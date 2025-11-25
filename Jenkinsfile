pipeline {
    agent any

    parameters {
        string(name: 'S3_BUCKET', defaultValue: 'simplecicd-artifacts-vino', description: 'S3 bucket to upload artifact')
        string(name: 'AWS_REGION', defaultValue: 'ap-south-1', description: 'AWS region for S3')
    }

    environment {
        BUILD_CONFIGURATION = "Release"
        OUTPUT_DIR = "publish"
        ARTIFACT_NAME = "simplecicd.zip"
        API_PROJECT = "SimpleCICD.Api\\SimpleCICD.Api.csproj"
        TEST_PROJECT = "SimpleCICD.Tests\\SimpleCICD.Tests.csproj"
    }

    stages {
        stage('Checkout') { steps { checkout scm } }

        stage('Restore & Build') {
            steps {
                bat 'dotnet --info'
                bat "dotnet restore \"%API_PROJECT%\""
                bat "dotnet restore \"%TEST_PROJECT%\""
                bat "dotnet build \"%API_PROJECT%\" -c %BUILD_CONFIGURATION% --no-restore"
                bat "dotnet build \"%TEST_PROJECT%\" -c %BUILD_CONFIGURATION% --no-restore"
            }
        }

        stage('Test') {
            steps {
                bat "dotnet test \"%TEST_PROJECT%\" -c %BUILD_CONFIGURATION% --no-build --verbosity normal"
            }
        }

        stage('Publish') {
            steps {
                bat "dotnet publish \"%API_PROJECT%\" -c %BUILD_CONFIGURATION% -o %OUTPUT_DIR%"
                bat """
                    powershell -Command "if (Test-Path '${ARTIFACT_NAME}') { Remove-Item '${ARTIFACT_NAME}' -Force }; Compress-Archive -Path ${OUTPUT_DIR}\\\\* -DestinationPath ${ARTIFACT_NAME} -Force"
                """
            }
        }

        stage('Replace Secret Placeholder') {
            steps {
                withCredentials([string(credentialsId: 'MY_API_KEY', variable: 'MY_API_KEY')]) {
                    bat '''
                        powershell -Command "(Get-Content \"%OUTPUT_DIR%\\appsettings.json\") -replace '\\$\\{API_KEY_PLACEHOLDER\\}', '$env:MY_API_KEY' | Set-Content \"%OUTPUT_DIR%\\appsettings.json\""
                    '''
                    bat """
                        powershell -Command "if (Test-Path '${ARTIFACT_NAME}') { Remove-Item '${ARTIFACT_NAME}' -Force }; Compress-Archive -Path ${OUTPUT_DIR}\\\\* -DestinationPath ${ARTIFACT_NAME} -Force"
                    """
                }
            }
        }

        stage('Archive Artifact') {
            steps {
                archiveArtifacts artifacts: "${ARTIFACT_NAME}", fingerprint: true
            }
        }

        stage('Upload to S3') {
            steps {
                // Using a "Username with password" credential in Jenkins where:
                // username = AWS_ACCESS_KEY_ID, password = AWS_SECRET_ACCESS_KEY
                withCredentials([usernamePassword(credentialsId: 'aws-credentials-id', usernameVariable: 'AWS_ACCESS_KEY_ID', passwordVariable: 'AWS_SECRET_ACCESS_KEY')]) {
                    echo "Uploading ${ARTIFACT_NAME} to s3://${params.S3_BUCKET}/ in region ${params.AWS_REGION}"
                    // Use same bat so env vars exist for the aws call
                    bat """
                        set AWS_ACCESS_KEY_ID=%AWS_ACCESS_KEY_ID%
                        set AWS_SECRET_ACCESS_KEY=%AWS_SECRET_ACCESS_KEY%
                        set AWS_DEFAULT_REGION=${params.AWS_REGION}
                        aws s3 cp "%WORKSPACE%\\${ARTIFACT_NAME}" "s3://${params.S3_BUCKET}/${ARTIFACT_NAME}" --region ${params.AWS_REGION}
                    """
                }
            }
        }
    }

    post {
        success { echo "Pipeline finished; artifact uploaded to s3://${params.S3_BUCKET}/${ARTIFACT_NAME}" }
        failure { echo "Pipeline failed — check console output." }
    }
}
