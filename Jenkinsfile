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
          // Log into Docker Hub
          sh 'echo $DOCKER_HUB_PSW | docker login -u $DOCKER_HUB_USR --password-stdin'

          // Analyze and fail on critical or high vulnerabilities
          // sh 'docker-scout cves $IMAGE_TAG --exit-code --only-severity critical,high'
          sh 'docker-scout cves $IMAGE_TAG --only-severity critical,high'
        }
      }
    }
    stage('Binary SCA analysis with Trivy') {
      steps {
        script {
          sh 'echo Trivy check'
          // sh 'trivy image --exit-code 1 --severity CRITICAL,HIGH $IMAGE_TAG'
          sh 'trivy image -f json -o results.json --severity CRITICAL,HIGH $IMAGE_TAG'
        }
      }
    }
    stage('DefectDojoPublisher') {
      steps {
        withCredentials([string(credentialsId: 'DEFECT_DOJO_KEY', variable: 'API_KEY')]) {
          defectDojoPublisher(artifact: 'results.json', productName: 'SDL6', scanType: 'Dependency Check Scan', engagementName: 'ci/cd', defectDojoCredentialsId: 'e423bc1971dfb2666b5166a2dded5d39aeeded2b', defectDojoUrl: 'http://localhost:8082', sourceCodeUrl: 'https://github.com/AlexDemokidov/sdl6.git', branchTag: 'main')
        }
      }
    }
  }
}
