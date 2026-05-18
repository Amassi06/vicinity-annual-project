plugins {
    application
    id("org.openjfx.javafxplugin") version "0.1.0"
}

repositories {
    mavenCentral()
}

java {
    toolchain {
        languageVersion.set(JavaLanguageVersion.of(21))
    }
}

application {
    mainClass.set("com.vicinity.desktop.VicinityApp")
}

javafx {
    version = "21.0.2"
    modules = listOf("javafx.controls")
}

tasks.withType<JavaCompile>().configureEach {
    options.encoding = "UTF-8"
}
