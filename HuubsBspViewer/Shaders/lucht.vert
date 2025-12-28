#version 330 core
layout (location = 0) in vec3 aPos;
out vec3 TexCoords;

uniform mat4 projection;
uniform mat4 view;

void main() {
    TexCoords = aPos;
    
    // VERWIJDER POSITIE: mat3 verwijdert de laatste kolom (de locatie)
    mat4 viewRotationOnly = mat4(mat3(view)); 
    
    vec4 pos = projection * viewRotationOnly * vec4(aPos, 1.0);
    
    // DE TRUC: Zet z gelijk aan w, zodat diepte altijd 1.0 is
    gl_Position = pos.xyww;
}
