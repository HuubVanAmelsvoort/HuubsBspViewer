#version 330 core
out vec4 FragColor;
in vec3 TexCoords;
uniform samplerCube skybox;

void main() {    
    //FragColor = vec4(TexCoords, 1.0);
// Teken lijnen op de randen van de kubus
    vec3 grid = step(0.95, abs(TexCoords));
    if(grid.x + grid.y + grid.z > 1.0) {
        FragColor = vec4(1.0, 1.0, 1.0, 1.0); // Witte randen
    } else {
        FragColor = vec4(TexCoords * 0.5 + 0.5, 1.0); // Gekleurde vlakken
    }
    //FragColor = vec4(0.0, 0.0, 1.0, 1.0); // Maak de lucht knalrood
    FragColor = texture(skybox, TexCoords);
}
