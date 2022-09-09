# Unity-MeshDeformation-Samples
A collection of samples on how to deform a mesh in Unity:
- Single-threaded
- Job System
- MeshData API
- Compute shader
- Vertex shader

## Performance Comparison

Configuration:
```
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
NVIDIA GeForce GTX 1070
```
Unity 2021.3.0f1

Windows standalone build.

The mesh under test has 160801 vertices and 320000 triangles.

|           Method |   Median |      Dev |   StdDev |      Min |      Max |
|------------------|---------:|---------:|---------:|---------:|---------:|
| VertexShader     | 0,99 ms  | 0,44 ms  | 0,44 ms  | 0,79 ms  | 8,23  ms |
| ComputeShader    | 2,32 ms  | 1,97 ms  | 4,55 ms  | 1,48 ms  | 17,11 ms |
| MeshData         | 9,32 ms  | 0,09 ms  | 0,80 ms  | 8,21 ms  | 14,28 ms |
| JobSystem        | 10,27 ms | 0,15 ms  | 1,50 ms  | 7,02 ms  | 19,65 ms |
| SingleThreaded   | 22,61 ms | 0,03 ms  | 0,69 ms  | 22,00 ms | 27,73 ms |
