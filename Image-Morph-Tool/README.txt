What doesn't work:
- Rotating an entire line (lines can still be rotated, but user must
move the selected line's start and end point in the direction they want to rotate)
- Not developed with SSE/OpenCL/Android in mind

What seems to work:
- Bidirectional morphing (forward and reverse)
- Benchmarking and threading implementation
- Morphing animation from source to destination, and destination to source

How to use:
1) Select source and destination images
2) Draw lines on either source or destination images.
3) Adjust the lines to allow background calculations to correctly determine direction of warp
(not adjusting the lines will result in a simple cross dissolve between images)
4) Select the number of frames, duration of animation, whether 