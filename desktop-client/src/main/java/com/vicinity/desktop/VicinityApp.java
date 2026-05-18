package com.vicinity.desktop;

import javafx.application.Application;
import javafx.scene.Scene;
import javafx.scene.control.Label;
import javafx.scene.layout.StackPane;
import javafx.stage.Stage;

/**
 * Coquille JavaFX Vicinity pour l’outil admin hors-ligne (Épice 14 simplifiée).
 */
public final class VicinityApp extends Application {

    @Override
    public void start(final Stage primaryStage) {
        final Label label =
                new Label("Vicinity — bureau admin (JDK 21 + JavaFX, squelette)");

        primaryStage.setTitle("Vicinity");
        primaryStage.setScene(new Scene(new StackPane(label), 520, 360));
        primaryStage.show();
    }

    public static void main(final String[] args) {
        launch(args);
    }
}
