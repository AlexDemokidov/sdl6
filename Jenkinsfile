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
        script {
          sh 'echo Push image to a Docker Hub'
          docker.withRegistry('', 'alexdemokidov') {
            app.push("latest")
          }
        }
      }
    }
  }
}
