pipeline {
    agent any   // change to agent { label 'windows' } if you have a labeled windows node

    environment {
        BUILD_CONFIGURATION = "Release"
        OUTPUT_DIR = "publish"
        ARTIFACT_NAME = "simplecicd.zip"
        API_PROJECT = "SimpleCICD.Api\\SimpleCICD.Api.csproj"
        TEST_PROJECT = "SimpleCICD.Tests\\SimpleCICD.Tests.csproj"
    }

    stages {

        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Restore & Build') {
            steps {
                echo "Restoring and building specific projects..."
                bat 'dotnet --info'

                // Restore specific projects
                bat "dotnet restore \"%API_PROJECT%\""
                bat "dotnet restore \"%TEST_PROJECT%\""

                // Build the API project
                bat "dotnet build \"%API_PROJECT%\" -c %BUILD_CONFIGURATION% --no-restore"

                // Build the TEST project so the test dll exists for dotnet test
                bat "dotnet build \"%TEST_PROJECT%\" -c %BUILD_CONFIGURATION% --no-restore"
            }
        }

        stage('Test') {
            steps {
                echo "Running tests..."
                // Now the test assembly will exist because we built the test project above.
                bat "dotnet test \"%TEST_PROJECT%\" -c %BUILD_CONFIGURATION% --no-build --verbosity normal"
            }
        }

        stage('Publish') {
            steps {
                echo "Publishing API project to folder: ${env.OUTPUT_DIR}"
                // Publish the API project (path to project file)
                bat "dotnet publish \"%API_PROJECT%\" -c %BUILD_CONFIGURATION% -o %OUTPUT_DIR%"

                // Zip the published output
                bat """
                    powershell -Command "if (Test-Path '${ARTIFACT_NAME}') { Remove-Item '${ARTIFACT_NAME}' -Force }; Compress-Archive -Path ${OUTPUT_DIR}\\\\* -DestinationPath ${ARTIFACT_NAME} -Force"
                """
            }
        }

        stage('Replace Secret Placeholder') {
            steps {
                withCredentials([string(credentialsId: 'MY_API_KEY', variable: 'MY_API_KEY')]) {
                    echo "Injecting Jenkins secret into ${env.OUTPUT_DIR}\\appsettings.json..."

                    // Replace placeholder in the published appsettings.json
                    bat '''
                        powershell -Command "(Get-Content \"%OUTPUT_DIR%\\appsettings.json\") -replace '\\$\\{API_KEY_PLACEHOLDER\\}', '$env:MY_API_KEY' | Set-Content \"%OUTPUT_DIR%\\appsettings.json\""
                    '''

                    // Recreate the ZIP so it contains the replaced file
                    bat """
                        powershell -Command "if (Test-Path '${ARTIFACT_NAME}') { Remove-Item '${ARTIFACT_NAME}' -Force }; Compress-Archive -Path ${OUTPUT_DIR}\\\\* -DestinationPath ${ARTIFACT_NAME} -Force"
                    """
                }
            }
        }

        stage('Archive Artifact') {
            steps {
                echo "Archiving artifact..."
                archiveArtifacts artifacts: "${ARTIFACT_NAME}", fingerprint: true
            }
        }
    }

    post {
        success {
            echo "Pipeline completed successfully!"
        }
        failure {
            echo "Pipeline FAILED — check logs."
        }
    }
}
