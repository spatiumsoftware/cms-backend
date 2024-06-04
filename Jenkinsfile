pipeline {
    agent any
 
    stages {
        stage('checkout') {
            steps {
                git branch: 'master', credentialsId: 'github', url: 'https://github.com/spatiumsoftware/cms-backend.git'   
                echo 'checkouting...'
            }
             post {
                always {
                    echo "This block always runs after this stage."
                }
            }
        }
        stage('Building') {
            steps {
                sh 'dotnet build'
            }     
                 post {
                    success {
                    slackSend message: 'this pipeline has been build sucessfully '
                    } 
                }
        }
        stage('Restoring') {
            steps {
                sh 'dotnet restore'
                echo 'restoring.....'
            }
        }
        stage('Publishing') {
            steps {
                sh 'dotnet publish -c Release -o ./publish'  
                echo 'publishing ...'
            }
             post {
                unstable {
                    echo "This block runs when the status of this stage is marked unstable."
                }
            }
        }
        stage("Build Docker Image"){
            steps{
                sh 'docker build -t abdelrahman9655/cms-backend:$BUILD_NUMBER  -f  Dockerfile .'
            }
        }
        stage('Login To Dockerhub'){
            steps{
                withCredentials([usernamePassword(credentialsId:'dockerhub-cred', usernameVariable:'USERNAME', passwordVariable: 'PASSWORD')]){
                sh'echo $PASSWORD | docker login -u $USERNAME --password-stdin'
                }
            }
        }
        stage("Push Docker Image"){
            steps{
                sh 'docker push abdelrahman9655/cms-backend:$BUILD_NUMBER'
            }
             post {
                success {
                    echo "This block runs when the stage succeeded."
                }
            }
        }
        stage('Delete Local Image') {
            steps {
                sh 'docker rmi abdelrahman9655/cms-backend:$BUILD_NUMBER'
            }
        }
    }
    post{
        always{
            sh 'docker logout'
        }
    }
}
