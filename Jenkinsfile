pipeline {
    agent any
    stages {
        stage('checkout') {
            steps {
                git branch: 'master', credentialsId: 'github-cred', url: 'https://github.com/spatiumsoftware/cms-backend.git'   
                echo 'checkouting...'
            }
        }
        stage('Building') {
            steps {
                sh 'dotnet build'
                echo 'Building...'
            }
        }
        stage('Testing') {
            steps {
                sh 'dotnet test'
                echo 'testing...'
            }
        }
        stage('Restoring') {
            steps {
                sh 'dotnet restore'
                echo 'restoring...'
            }
        }
        stage('Publishing') {
            steps {
                sh 'dotnet publish -c Release -o ./publish'  
                echo 'publishing ...'
            }
        }
    }
    
}
