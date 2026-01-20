#version 330 core
out vec4 FragColor;
in vec2 texCoord;

// Uniforms zijn variabelen die van buitenaf worden doorgegeven aan de shader
// Hier definiëren we enkele uniform variabelen
// die we later in de shader kunnen gebruiken
// Het zetten gebeurt in de C# code, bijvoorbeeld:
// GL.Uniform1(GL.GetUniformLocation(_shader.Handle, "isMarker"), 0); // zet 0 in de isMarker

uniform sampler2D texture0;
uniform vec4 markerKleur;
uniform vec4 debugColor;
uniform int debugMode;
uniform int isMarker; 

void main() {
  if(isMarker == 1) {
    // Als het een marker is, negeer alle andere logica
    FragColor = markerKleur;
  } else if(debugMode == 1) {
    FragColor = debugColor;
  } else {
    // Normale map rendering
    vec4 plaatje = texture(texture0, vec2(texCoord.x, 1.0 - texCoord.y));

    if(plaatje.a < 0.1) {
      FragColor = vec4(texCoord, 0.0, 1.0);
    } else {
      FragColor = plaatje;
    }
  }
}
