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
        sh 'echo Build application image'
        def app = docker.build("pinger", "./dotnet/PgConnect")
      }
    }
  }
}
