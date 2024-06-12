pipeline {
  agent any
  stages{
    stage('Clone Git repository') {
      steps {
        checkout scm
      }
    }
    stage('SonarQube analysis') {
      steps {
        script {
          sh 'echo Run SAST - SonarQube analysis'
          def scannerHome = tool 'sonar_scanner';
          withSonarQubeEnv() {
            sh "${scannerHome}/bin/sonar-scanner -Dsonar.projectKey=SDL6"
          }
        }
      }
    }
    stage("SonarQube Quality Gate") {
      steps {
        waitForQualityGate abortPipeline: true
      }
    }
    stage('Build Image') {
      steps {
        script {
          sh 'echo Build application image'
          app = docker.build("alexdemokidov/pinger", "./dotnet/PgConnect")
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
