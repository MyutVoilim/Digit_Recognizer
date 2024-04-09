# Table of Contents

# Introduction
## AI Digit Recognizer
![Digit Recognizer 2](https://github.com/MyutVoilim/Projects-Portfolio/assets/54462267/b6f11075-d122-4dbc-96d6-03a7fa03f8d2)

<b>Technologies:</b> C# | WPF | .Net Framework | Visual Studio

<b>Overview:</b> Utilizing the power of machine learning to recognize hand-drawn digits, this complex passion project is a from-scratch feed-forward neural network that implements backpropagation for error correction and it is the culmination of my journey as a developer and problem-solver. It not only showcases my proficiency in C#, OOP, and machine learning but also reflects my growth in planning, designing, and testing complex systems. Built with WPF in the .Net framework, this application offers an engaging and intuitive user experience.

<b>Features:</b>
- <b>Interactive Design:</b> allowing the user to draw or load digits with real-time confidence value feedback.
- <b>Customizable Neural Networks:</b> featuring the ability to create custom neural networks by defining hidden layer sizes and node counts.
- <b>Adjustable Training Parameters:</b> Control over training parameters such as learning rates and epoch counts to influence training route.
- <b>User-Friendly Interface:</b> Including dynamic visuals, progress bars, accuracy tracking, and confidence value displays.
- <b>Model Management:</b> Save, load, and swap out training data for greater flexibility and experimentation.

<b>Introduction to Me and the Project</b>

https://github.com/MyutVoilim/Projects-Portfolio/assets/54462267/df073c76-e715-49eb-9d00-9c8d300d937c

<b>Project Demo</b>

https://github.com/MyutVoilim/Projects-Portfolio/assets/54462267/197f9e12-64d2-480d-b767-73c9e4ce858b

<b>Challenges Overcome:</b>
- <b>Comprehensive Design and Development:</b> navigated the complexities of implementing a neural network in C#, applying OOP principles to create a robust system.
- <b>Testing and Reliability:</b> : Employed rigorous unit testing to ensure system reliability, which was crucial in identifying and resolving challenging bugs related to the complex journey data takes from the input, processing, backpropagation, and eventually the output.
- <b>Documentation and Learning:</b> The project was not just a technical challenge but also an opportunity to refine my skills in creating comprehensive documentation and understanding the intricacies of neural network behaviors.

# Getting Started

# Usage

# Architecture and Design
	(Motive project overview)
It was important for this project to represent the skills I’ve attained over years through personal and academic software development. Not only to showcase my abilities in the development process but to challenge my comfort zone with a complex objective. This would refine my techniques and help snuff out issues improving my skill set. With that sentiment and the growing importance of artificial intelligence, in particular neural networks, I thought no better challenge than to create me own neural network from scratch. I chose to implement a neural network to detect hand drawn digits on a 28 x 28 grid. This meant creating a from scratch feed-forward neural network that utilized backpropagation and gradient descent for error correction. The program includes an interactive, responsive, and customizable user experience. This includes the ability to draw digits with real-time guesses from the network. The ability to start and stop training with accuracy tracking of the current learning cycle. Customization in training parameters such as epoch and learning rate. As well as the ability for the user to define their own structure for new neural networks involving the ability to define layer and node counts and of course the ability to save trained networks as well as load old ones.

	(Environment/Tools)
For an IDE I chose Visual Studio 2022 for its rich feature set of diagnostic tools, integrated version controlling, and CI/CD capabilities. Along side my IDE I used GitHub for version controlling to create changes within my main and experimental branches. This is an indispensable tool for testing experimental updates before pushing to the main branch and avoiding destructive alterations the can hurt functionality or create bugs. GitHub allows for a remote repository for an off-side code base that adds safety and remote capabilities. I also benefited form the Microsoft testing framework within Visual Studio to create unit tests to manage more complex classes such the neural network. Utilizing these tools I approached my development with continuous iterative updates to test changes on my experimental branch with the assistance of my unit tests before pushing more complete functionality the main branch.


	(UI overview)
The project is broken down into two main components, The UI and the neural network logic. With the UI I wanted to make an experience of more than just using watching a preexisting neural network’s accuracy rating. This meant creating an experience that is interactive and interesting to mess around with. For engagement I implemented the 28 x 28 grid that would allow the user to draw and test their own digits and see how the AI fairs in guessing it. To add a more interesting environment I allowed the ability for the user to create their own neural network structure with the power to change various components such as how many layers there are and the node count within each of those layers. Combined with the ability to seamlessly start and stop training I hoped that this would create a fun test bed encouraging the user to experiment. Of course to further facilitate this ability it was important to add control over how training is directed through the option of changing the learning rate and epoch count for new or existing networks. If the user is proud of a particular network they of course have the ability to save and load the networks as they please. To create engaging visuals I included the network’s real-time confidence values for any drawn or loaded digit that shows an inside look into how much the current digit looks like any particular number. The accuracy tracker provided gives valuable information to the user to monitor progress but also adjust learning parameters as needed. Progress of the current cycle is important so a progress bar is provided for meaningful user feedback.


	
	(difficulties in neural network design)
While its important to have UI elements for a coherent design and facilitate enjoyable user interactions, there is a complex backbone that runs the neural network and the process that must occur for this all to function in tandem. In this more technical component I’m going to focus on the process of the neural network. I will go over several major components to which I will mention A brief overview of the difficulties around neural network design, what I implemented and it purpose, snippets of the code, as well as considerations and pitfalls in my implementation.


This project brought about unique challenging in being the first time I’ve created a true neural network, but also difficulties from the inherit complexity that a neural network brings in both design and testabiltiy challenges. The journey inputs takes to pass through the neural network and eventually emerge as a result consist of many components that are each prone to difficult to spot bugs and errors. Much like if you’ve ever built a PC from scratch, if after building the whole thing your left with a PC that doesn’t turn on, well, that could be a multitude of things. Was it the RAM, the CPU, the GPU, the power supply, was the monitor not working, did you just forget to plug something in? A neural network faces similar difficulties. Their are so many components working in conjunction to mutate and refine data further along until eventually you getting an output. You can see parts of that process in the chart below. From converting input data into something the network can use, normalization, a pass through the network, applying and initializing of weights and biases, backpropagation, loss function, gradient-descent, and the eventual output, so much can go wrong.
 
![FNN Diagram](https://github.com/MyutVoilim/AI-Digit-Recognition/assets/54462267/52fa5e1b-93c0-4800-bac6-708e10506cc9)

Now going over the specific methods that make up the neural network, it’s purpose, and considerations.


(Normalization)

Having input data be within the functional range of the activation function is important, especially considering my input range is between 0 and 255. Normalization helps scale the input values to something the network can more easily work with before passing it into calculations. In my case I don’t really have to worry about complexity since my system has simple values from black (0) or white (255). As such, just dividing the input value by the max number possible of 255 puts these value withing a useful range of 0 and 1. In conjunction with the weights and biases it is important to have relevant values to ensure the input has performative influence on the network.

![Normalization](https://github.com/MyutVoilim/AI-Digit-Recognition/assets/54462267/3fce2671-960a-4188-85c2-e1bad6669040)

(activation function)

The sigmoid function is utilized as the activation function across the neural network to convert the sum of normalized inputs, weights, and biases into an output between 0 and 1. This assists in classification as this allows the output to be represented in a probabilistic way. Sigmoid is also non-linear which can assist in detecting complex patterns during training. The sigmoid function allows for a smooth gradient during backpropagation that allows for small changes in the system and thus effective training.

![Sigmoid](https://github.com/MyutVoilim/AI-Digit-Recognition/assets/54462267/d4f86f72-0ce7-4c03-a4eb-c1eece6530be)

However, sigmoid is susceptible to the vanishing gradient problem, where due to how gradient-based backpropagation works, large values will be bounded while derivatives of that large value will be used during backpropagation and cause very small and insignificant changes to the system. For this reason sigmoid only has a functional range of about -4 and 4 before it becomes harmful towards learning performance and thus necessitates careful consideration for what values are used during initialization and normalization. In my case my weights are initialized around these considerations, but since my input data is guaranteed to be positive it causes my data to be positive prone. This may causes slight performance issues with sigmoid since it pushes that values to be closer to 1 rather than centered at 0.
Instead activation functions like the Rectified Linear Unit (ReLU) function can be used that allow for very large values to still have an impact on learning. However, this involves reconsideration in how values are initialized and normalized.

(weights and biases)
The initialization process sets the starting values for the weights and biases within the network. Weights define the strength of connections between nodes of one layer and all nodes of the subsequent layer. In my case the weights are initialized with random values between -1 and 1, ensuring that the data fed into the network falls within a functional range of sigmoid (-4, 4). Biases determine how significant a node should be within the network by adding a flat value independent of the weights. biases are initialized to 0 allowing their influence on the network to evolve during the training process.

![WeightInitalize](https://github.com/MyutVoilim/AI-Digit-Recognition/assets/54462267/f2e704c9-3975-4ab6-bfc6-055b16443bea)

This method of initialization is critical for producing an effective environment for the neural network to learn and converge during training. Randomizing weight values breaks symmetry preventing neurons within the same layer from learning in an identical manner and instead allow for diversification.
As mentioned already, initialization must follow the effective range and properties of the activation function used in conjunction with how normalized data is added along with the weights and biases. Values too high and sigmoid will run into the vanishing gradient problem and hurt training performance.


(Forward Pass and Training)
With inputs normalized and values initialized, data can finally take a full pass through each layer of the network with weights and biases applied until the eventual output layer.  The values of that output layer are passed through the activation function sigmoid to give a 0 to 1 probability for each number, useful for classification. Their are many architectures for how layers should interact with each other but I opted for a feed-forward neural network that does a single pass from one layer to the next.

![Forward Pass](https://github.com/MyutVoilim/AI-Digit-Recognition/assets/54462267/0494532a-30c8-4d52-aa42-62a75578e362)

In order to initiate training a methodology must be adopted. I utilized a variant of a Stochastic Gradient Descent (SGD) approach to compare and train the network on a per image basis. This process updates parameters using a gradient descent function on mini batches, in my case a batch of 1. This has the benefit of frequent adjustments and a straight forward implementation, although, there are some drawbacks that I’ll mention later. With the output calculated and the network’s guess compared to the expected values, it is sent for error correction. 

Error correction is achieved through three components, a loss function that determines the degree of error, backpropagation utilized to traverse backwards from the output layer up through all the hidden layers, and a gradient descent algorithm that updates the weights and biases with each update getting progressively smaller as it moves up the hidden layers during backpropagation. The length and rate for training is determined by the user through epoch count and learning rate. The learning rate has significant effects for the behavior of the convergence speed and landing at the minimum. Large rates causing quick but unstable convergence that may overshoot the minimum and fail to ever fully converge while smaller rates are slower and risk getting trapped in local minima.

 ![Backpropagation](https://github.com/MyutVoilim/AI-Digit-Recognition/assets/54462267/70e91e2e-71d4-4be2-9fe0-bdb023ba949c)

Their are several components of my approach that likely limit performance. Firstly, the use of a single image SGD approach may lead to over adjustments during error correction that the use of larger mini batches could help avoid. Single images also limit the computation efficiency that parallel processing of mini batches would provide. My current approach with the training data is to run through the entire set once which could easily lead to overfitting. Adding variance to how training data is passed through along with additional augmentation such as slight rotations, translations, and noise to the images would allow for a more effective approach to new inputs. 

When it comes to the error correction, my loss function is not a traditional approach. I realized on analysis that the loss function used is actually only applied on the output layer when traditionally it would be calculated on a per layer basis. The system still seems to be effective for my application, however, it is only a partial loss function that could be improved. While I do use sigmoid to give output probabilities between 0 and 1, it may not be fully utilized since I do not apply a softmax distribution to the output layer. What this means is that the probability distribution of the system may equal greater than 1 with all outputs combined instead of being a classical probability total of 1. This could hurt the nature of how the system makes adjustments as well as make it difficult to work with other loss functions that rely on that foundation such as cross-entropy. The use of a static learning rate is also limiting as a more adaptive approach could allow for initially larger adjustments for quick convergence and later shift to a slower rate to complete convergence onto the minimum.

My main focus with this project was to take on a challenge while critically analyzing and documenting my process. Their were a million things I changed and adjusted while developing and, as mentioned throughout this documentation, a million more that I could have changed. There is a difficult line that must be drawn in software development for when to “complete” a project, especially in the context of something with limitless potential as a neural network. I the real world that Is usually dead lines and development power, in my case it was when to stop and analyze. What was most important was to try and understand the processes that make this neural network work and what pit falls and potential improvements could be made. 

I think that the nature of how neural networks even come to “learn”  and retain information is incredibly interesting and problematic. It is not always straight forward how information is created/stored and the effects adjustments have on this process when making improvements to a system. Their are decades of empirical evidence for effective methodologies to increase performance and continuous attempts to understand the true nature of how a neural networks operate. However, increasing the complexity of the architecture, methodologies, and scale of these systems creates a fuzzy black box of processes that work in ways we may not be able to comprehend, even if we claim we do. I learned so much in creating this from helping push my fundamentals to learning about complex systems, and most importantly my weaknesses and what I can strife to improve.
It takes uncomfortable challenges to spot weaknesses. I bring that mentality to everything I do and is one of the best ways to grow as a person. If I had to remake this project I would change things such as architecture of the neural network and more direct unit testablity. Now understanding better each component that must exist, I have better insight to make methods less dependent on exact implementations and allow for modularity with methods like normalization, passthrough, and error correction. This would allow flexibility in how data is processed potentially allowing the user to switch out and test as the wish with different methodologies is error correction, activation function, and different architecture.
 
# Code Examples

# FAQs

# Acknowledgement



# Key Concepts
- Purpose
- Chanlleges
- DRY, SOLID, Encapculation
- Unit Testing
- Error handling
- polymorphism
- Inheritence
- FNN
- Back propigation
- Sigmoid
- Dependency injection
- Readablity and documentation
- OOP
- Architecture such as MVC
- what I learned, what I found to be a problem, what I could improve
- use of C# and WPF and the .net framework

