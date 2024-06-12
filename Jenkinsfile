pipeline {
  agent any

  environment {
    DEBRICKED_TOKEN = credentials('DEBRICKED_TOKEN')
  }
  
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
    stage('Debricked Scan') {
      steps {
        script {
          sh 'curl -L https://github.com/debricked/cli/releases/download/release-v1/cli_linux_x86_64.tar.gz | tar -xz debricked'
          sh './debricked scan'
        }
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
    stage('Trivy analysis') {
      steps {
        script {
          sh 'echo Trivy check'
          // sh 'trivy image --exit-code 1 --severity CRITICAL,HIGH alexdemokidov/pinger:latest'
          sh 'trivy image alexdemokidov/pinger:latest'
        }
      }
    }
  }
}
