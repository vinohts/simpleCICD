pipeline {
    agent any

    environment {
        BUILD_CONFIGURATION = "Release"
        OUTPUT_DIR = "publish"
        ARTIFACT_NAME = "simplecicd.zip"
        API_PROJECT = "SimpleCICD.Api\\SimpleCICD.Api.csproj"
        TEST_PROJECT = "SimpleCICD.Tests\\SimpleCICD.Tests.csproj"
        DEST_DIR = "D:\\temp"               // destination on the agent where we will copy artifact
    }

    stages {
        stage('Checkout') {
            steps { checkout scm }
        }

        stage('Restore & Build') {
            steps {
                echo "Restoring and building projects..."
                bat 'dotnet --info'
                bat "dotnet restore \"%API_PROJECT%\""
                bat "dotnet restore \"%TEST_PROJECT%\""
                bat "dotnet build \"%API_PROJECT%\" -c %BUILD_CONFIGURATION% --no-restore"
                bat "dotnet build \"%TEST_PROJECT%\" -c %BUILD_CONFIGURATION% --no-restore"
            }
        }

        stage('Test') {
            steps {
                echo "Running tests..."
                bat "dotnet test \"%TEST_PROJECT%\" -c %BUILD_CONFIGURATION% --no-build --verbosity normal"
            }
        }

        stage('Publish') {
            steps {
                echo "Publishing API project to folder: ${env.OUTPUT_DIR}"
                bat "dotnet publish \"%API_PROJECT%\" -c %BUILD_CONFIGURATION% -o %OUTPUT_DIR%"
                // create zip of publish folder
                bat """
                    powershell -Command "if (Test-Path '${ARTIFACT_NAME}') { Remove-Item '${ARTIFACT_NAME}' -Force }; Compress-Archive -Path ${OUTPUT_DIR}\\\\* -DestinationPath ${ARTIFACT_NAME} -Force"
                """
            }
        }

        stage('Replace Secret Placeholder') {
            steps {
                withCredentials([string(credentialsId: 'MY_API_KEY', variable: 'MY_API_KEY')]) {
                    echo "Injecting secret into ${env.OUTPUT_DIR}\\appsettings.json..."
                    bat '''
                        powershell -Command "(Get-Content \"%OUTPUT_DIR%\\appsettings.json\") -replace '\\$\\{API_KEY_PLACEHOLDER\\}', '$env:MY_API_KEY' | Set-Content \"%OUTPUT_DIR%\\appsettings.json\""
                    '''
                    // recreate zip so it contains replaced file
                    bat """
                        powershell -Command "if (Test-Path '${ARTIFACT_NAME}') { Remove-Item '${ARTIFACT_NAME}' -Force }; Compress-Archive -Path ${OUTPUT_DIR}\\\\* -DestinationPath ${ARTIFACT_NAME} -Force"
                    """
                }
            }
        }

        stage('Archive Artifact') {
            steps {
                echo "Archiving artifact to Jenkins"
                archiveArtifacts artifacts: "${ARTIFACT_NAME}", fingerprint: true
            }
        }

        stage('Copy artifact to D:\\\\temp') {
            steps {
                echo "Copying ${ARTIFACT_NAME} to ${env.DEST_DIR} on agent"
                // Ensure destination exists and copy the artifact from the workspace root
                bat """
                    if not exist "${DEST_DIR}" (mkdir "${DEST_DIR}")
                    copy /Y "%WORKSPACE%\\${ARTIFACT_NAME}" "${DEST_DIR}\\${ARTIFACT_NAME}"
                """
            }
        }
    }

    post {
        success {
            echo "Pipeline completed successfully. Artifact copied to ${env.DEST_DIR}"
        }
        failure {
            echo "Pipeline FAILED — check logs."
        }
    }
}
