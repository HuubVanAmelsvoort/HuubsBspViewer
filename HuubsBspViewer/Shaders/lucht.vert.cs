#version 330 core
layout(location = 0) in vec3 aPos;
out vec3 TexCoords;

uniform mat4 projection;
uniform mat4 view;

void main() {
  // 1. Haal de aspect ratio correctie uit de projectie matrix
  // In een standaard perspectief matrix is projection[0][0] gelijk aan 1 / (tan(fov/2) * aspect)
  // We gebruiken dit om de breedte (X) van de TexCoords te herstellen.
  float correction = projection[0][0] / projection[1][1];

  // 2. Pas de correctie toe op de X-as (of Z/Y afhankelijk van je draaiing)
  // Als de lucht te breed is, moet je vermenigvuldigen of delen door 'correction'
  TexCoords = vec3(aPos.x * correction, aPos.z, -aPos.y);

  mat4 staticView = mat4(mat3(view));
  vec4 pos = projection * staticView * vec4(aPos, 1.0);
  gl_Position = pos.xyww;
}
