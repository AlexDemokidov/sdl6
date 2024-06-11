pipeline{
  agent any
  stages{
    stage('Clone Git repository') {
      steps {
        checkout scm
      }
    }
    stage('Build Image') {
      steps {
        script {
          sh 'echo Build application image'
          app = docker.build("pinger", "./dotnet/PgConnect")
        }
      }
    }
    stage('Push image to Docker Hub') {
      steps {
        sh 'echo Push image to a Docker Hub'
        docker.withRegistry('https://registry.hub.docker.com', 'alexdemokidov') {
          app.push("latest")
        }
      }
    }
  }
}
