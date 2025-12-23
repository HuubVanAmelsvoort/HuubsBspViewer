#version 330 core
out vec4 FragColor;
in vec2 texCoord;

uniform sampler2D texture0;

void main() {
  // Voor debug: als de texture niet werkt, zie je in ieder geval UV-kleuren
  vec4 tex = texture(texture0, 1.0 - texCoord); // 1.0 - draait texture om
  if(tex.a < 0.1) {
     FragColor = vec4(texCoord, 0.0, 1.0);
  } else {
     FragColor = tex;
  }
}    
