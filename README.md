# Greetwin version 1
------------------------
Copyright June 2023 (version 1): Jippe Hoogeveen, Rob H. Bisseling

------------------------
##### Brief Description

This solver uses a greedy approach to approximate the twinwidth. Everytime it chooses the pair (u, v) of vertices such that the vertex that results from their contraction has the least amount of red edges.
The basic algorithm just examines all possible pairs (u, v) of vertices at distance at most 2. Another version only examines pairs (u, v) with the property that u == v mod m or u + v == s mod m for some constants m and s. This significantly improves the performance of the algorithm, but it also increases the value of the final solution. Since speed is very important for the Pace challenge, we use this variant partially.
In the final algorithm for the challenge, we combine the variant with m and s with the basic variant (which is what you would get with m = 1). Based on the size of the graph, we estimate a good value of m. Then we do some contractions with a certain value of s and change s and so on. We also decide how long to continue with this value of s based on the time. In that way we try to get the best solution in 5 minutes.

##### Description on how to run the solver

This should not be a problem since we do not use any other systems or classes. All the code can be found in the Program.cs file. We can either read a graph from the console (which is how we read information in the challenge) or from a file. In bin/debug are many examples of graphs. Some of them are public instances of the Pace challenge while others are constructed by ourselves for testing the algorithm for the bachelor thesis.

##### Requirement on external libraries

There are not many external libraries used and they are all very basic (like System or System.Collections.Generic or System.IO).
