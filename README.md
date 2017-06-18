In order to run please choose the `Release | x86` target.
Also it's recommended for performance reasons to run without debugging.

This is something I did a long time ago and is released for the world
to learn from. This is a good way to learn about how real time 2D and 3D rendering works (in Video Games as opposed to Cinema).

# PolygonEditor

Full software imlpementation of line drawing and polygon filling

How to use:
===========
ESC - Clear
Left Click - Add point to current polygon
Right Click - Add final point to polygon
A - Change line drawing algorithm

# 3D Viewer

Full software implementation of 3D model rendering including:
- Full camera control
- Multiple projections
- Face Culling
- Z-Buffer
- Scissoring
- Multiple colored dynamic lights
- phong shading model

Patches for texturing are more than welcome.

How to use:
===========
Left Click + Drag - Translate X Y
Right Click + Drag - Rotate X Y
Mouse Wheel - Translate Z
[ - Scale x0.2
] - Scale x1.2
R - Reset transformation
W - Toggle wireframe
F - Toggle Fill
P - Change projection (current projection is displayed in the title)
M - Change Model (current model is displayed in the title)

The White light is fixed at (0, 0, -150)
The blue light followes the mouse.
