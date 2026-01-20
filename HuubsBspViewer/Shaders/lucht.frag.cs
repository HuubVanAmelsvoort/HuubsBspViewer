#version 330 core
out vec4 FragColor;
in vec3 TexCoords;

uniform samplerCube skybox;

void main() {
  //FragColor = vec4(0.0, 0.0, 1.0, 1.0); // Maak de lucht knalrood
  FragColor = texture(skybox, TexCoords);

  // testje witte en blauwe ruitjes
  //float grid = sin(TexCoords.x * 20.0) * sin(TexCoords.y * 20.0) * sin(TexCoords.z * 20.0);
  //if(grid > 0.0) FragColor = vec4(1.0); else FragColor = vec4(0.0, 0.0, 1.0, 1.0);
}
