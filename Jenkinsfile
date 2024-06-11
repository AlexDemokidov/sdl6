stage('Clone Git repository') {
checkout scm
}

stage('Build Image') {
sh 'echo Build application image'
app = docker.build("sdl6/dotnet/PgConnect", ".")
}
