pipeline {
    agent any   // Use Windows agent. If your agent has a label, replace with: agent { label 'windows' }

    environment {
        BUILD_CONFIGURATION = "Release"
        OUTPUT_DIR = "publish"
        ARTIFACT_NAME = "simplecicd.zip"
    }

    stages {

        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Restore & Build') {
            steps {
                echo "Restoring and building project..."
                bat 'dotnet --info'
                bat "dotnet restore"
                bat "dotnet build -c %BUILD_CONFIGURATION% --no-restore"
            }
        }

        stage('Test') {
            steps {
                echo "Running tests..."
                bat "dotnet test -c %BUILD_CONFIGURATION% --no-build --verbosity normal"
            }
        }

        stage('Publish') {
            steps {
                echo "Publishing project..."
                bat "dotnet publish SimpleCICD.Api -c %BUILD_CONFIGURATION% -o %OUTPUT_DIR%"

                // Create zip of publish folder
                bat """
                    powershell -Command "if (Test-Path '${ARTIFACT_NAME}') { Remove-Item '${ARTIFACT_NAME}' -Force }; Compress-Archive -Path ${OUTPUT_DIR}\\\\* -DestinationPath ${ARTIFACT_NAME} -Force"
                """
            }
        }

        stage('Replace Secret Placeholder') {
            steps {
                // Jenkins secret: MY_API_KEY
                withCredentials([string(credentialsId: 'MY_API_KEY', variable: 'MY_API_KEY')]) {
                    echo "Injecting Jenkins secret into appsettings.json..."

                    // Replace ${API_KEY_PLACEHOLDER} with actual secret
                    bat '''
                        powershell -Command "(Get-Content '%OUTPUT_DIR%\\appsettings.json') -replace '\\$\\{API_KEY_PLACEHOLDER\\}', '$env:MY_API_KEY' | Set-Content '%OUTPUT_DIR%\\appsettings.json'"
                    '''

                    // Recreate ZIP after replacement
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
