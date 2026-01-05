#version 330 core
out vec4 FragColor;
in vec2 texCoord;

uniform sampler2D texture0;
uniform vec4 markerKleur;
uniform vec4 debugColor; 
uniform int debugMode; 
uniform int isMarker; // TOEGEVOEGD: uniform keyword

void main() {
    if (isMarker == 1) {
        // Als het een marker is, negeer alle andere logica
        FragColor = markerKleur;
    } 
    else if (debugMode == 1) {
        FragColor = debugColor;
    } 
    else {
        // Normale map rendering
        vec4 tex = texture(texture0, vec2(texCoord.x, 1.0 - texCoord.y)); 
        
        if(tex.a < 0.1) {
             FragColor = vec4(texCoord, 0.0, 1.0);
        } else {
             FragColor = tex;
        }
    }
}