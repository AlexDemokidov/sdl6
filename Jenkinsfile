pipeline {
  agent any

  environment {
    DEBRICKED_TOKEN = credentials('DEBRICKED_TOKEN')
    DOCKER_HUB = credentials('alexdemokidov')
    IMAGE_TAG  = 'alexdemokidov/pinger:latest'
  }
  
  stages{
    stage('Clone Git repository') {
      steps {
        checkout scm
      }
    }
    stage('SAST SonarQube analysis') {
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
    stage('SCA with Debricked') {
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
    stage('Analyze image with Docker Scout') {
      steps {
        script {
          // Install Docker Scout
          sh 'curl -sSfL https://raw.githubusercontent.com/docker/scout-cli/main/install.sh | sh -s -- -b /usr/local/bin'

          // Log into Docker Hub
          sh 'echo $DOCKER_HUB_PSW | docker login -u $DOCKER_HUB_USR --password-stdin'

          // Analyze and fail on critical or high vulnerabilities
          sh 'docker-scout cves $IMAGE_TAG --exit-code --only-severity critical,high'
        }
      }
    }
    stage('Binary SCA analysis with Trivy') {
      steps {
        script {
          sh 'echo Trivy check'
          // sh 'trivy image --exit-code 1 --severity CRITICAL,HIGH alexdemokidov/pinger:latest'
          sh 'trivy image $IMAGE_TAG'
        }
      }
    }
  }
}
