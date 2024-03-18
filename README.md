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
I hope achieve two goals within this documentation, to describe the motivations and development of this project, as well as, a more critical analysis of the neural network.
	It was important for this project to represent the skills I’ve attained over years through personal and academic software development. Not only to showcase my abilities in the development process but to challenge my comfort zone in taking on a complex development challenge. This would not only further refine my techniques but also help snuff out issues in my development process for I can further improve.
	Like any project is was important to consider the software development life cycle in creating not only a functional program with all of its bells and whistles but and engaging experience for the user. With that sentiment and the growing importance of artificial intelligence in our lives, in particular neural networks, I thought no better challenge than to create me own neural network from scratch. For this I chose to implement and neural network to detect hand drawn digits on a 28x28 grid. This meant creating a from scratch feed-forward neural network that utilized backpropagation for error correction. The program includes an interactive, responsive, and customizable user experience. This includes the ability to draw digits with real-time guesses from the neural network. The ability to start and stop training with accuracy tracking of the current learning cycle. Customization in not only training parameters such as epoch and learning rate but the ability for the user to create their own structure for a new neural networks involving the ability to define layer and node counts and of course the ability to save trained networks as well as load old ones.


	This project brought about unique challenging in being the first time I’ve created a true neural network, but also difficulties from the inherit complexity that a neural network brings in both design challenges and testabiltiy. The journey inputs takes to pass through the neural network and eventually emerge as a result consist of many components that are each prone to difficult to spot bugs and errors. Much like if you’ve ever built a PC from scratch, if after building the whole thing your left with a PC that doesn’t turn on, well, that could be a multitude of things. Was it the RAM, the CPU, the GPU, the power supply, was the monitor not working, did you just forget to plug something in? A neural network faces similar difficulties. Their are so many components working in conjunction to mutate and refine data further along until eventually you getting an output. You can see parts of the process in the chart below. From converting input data into something the network can use, normalization, a pass through the network, applying and initializing of weights and biases, backpropagation, loss function, gradient-descent, and the eventual output, so much can go wrong.
 
![FNN Diagram](https://github.com/MyutVoilim/AI-Digit-Recognition/assets/54462267/52fa5e1b-93c0-4800-bac6-708e10506cc9)

 More than any other project I found myself dealing with sneaky and hard to find bugs that would have subtle but significant effects in each subsequent part of system. I realized just how important creating testable units of code were and implementing testability wherever possible. Version controlling was also utilized and helped implement new code and with issues arising during refactoring. Unit testing and version controlling not only helped with finding where to look for issues but assisted in preventing subtle bugs from emerging when refactoring code. In one such example I had been rewriting a loop in the backpropagation method and had accidentally set the loop to go through only a fraction of the neural network without realizing. I had also failed to make this particular method easy to test and had only set unit testing to check this method indirectly. What made this bug worse is that since a portion of the loop had been going through the data properly, the neural network was still functioning but now is was seemingly capping its accuracy at 80% when I had been getting 98% before. I scratched my head at this bug for a couple days testing so many different components and only found the issue thanks to version controlling and comparing the old functional code with current one. This was a lesson learned to not only why it was important to make everything easily testable but the subtlety that some bugs can have especially in my case where it didn’t overtly break everything.

While its important to have UI elements for the for coherent design and user interaction, there is a complex backbone that runs the neural network and the process that must occur for this all to work. In this more technical component I’m going to focus on the process of the neural network. I will go over several major components to which I will mention various aspects from what I implemented and it purpose, snippets of the code, as well as considerations and pitfalls my implementation may have. Those components will in order go normalization, activation function, initialization, forward pass, error correction, and output.

Normalization:

Having input data be within the functional range of the activation function is important, especially considering my input range is between 0 and 255. Normalization helps scale the input values to something the network can more easily work with before passing it into calculations. In my case I don’t really have to worry about complexity since my system has simple values from black (0) or white (255). As such, just dividing the input value by the max number possible of 255 puts these value withing a useful range of 0 and 1. In conjunction with the weights and biases it is important to have relevant values to ensure the input has performative influence on the network.

![Normalization](https://github.com/MyutVoilim/AI-Digit-Recognition/assets/54462267/3fce2671-960a-4188-85c2-e1bad6669040)

Use of the Sigmoid Activation Function

The sigmoid function is utilized as the activation function across the neural network to convert the sum of normalized inputs, weights, and biases into an output between 0 and 1. This assists in classification as this allows the output to be represented in a probabilistic way. Sigmoid is also non-linear which can assist in detecting complex patterns during training. The sigmoid function allows for a smooth gradient during backpropagation that allows for small changes in the system and thus effective training.

![Sigmoid](https://github.com/MyutVoilim/AI-Digit-Recognition/assets/54462267/d4f86f72-0ce7-4c03-a4eb-c1eece6530be)

Pitfalls and Considerations:
However, sigmoid is susceptible to the vanishing gradient problem, where due to how gradient-based backpropagation works, large values will be bounded while derivatives of that large value will be used during backpropagation and cause very small and insignificant changes to the system. For this reason sigmoid only has a functional range of about -4 and 4 before it becomes harmful towards learning performance and thus necessitates careful consideration for what values are used during initialization and normalization. In my case my weights are initilizaed around these consideration, but since my input data is guaranteed to be positive it causes my data to be positive prone. This may causes slight performance issues with sigmoid since it pushes that values to be closer to 1 rather than 0 where sigmoid values should be.
Instead activation functions like the Rectified Linear Unit (ReLU) function can be used that allow for very large values to still have an impact on learning. However, this involves reconsiderating how values are initialized and normalized.

Initialization of Weights and Biases
What:
The initialization process sets the starting values for the weights and biases within the network. Weights define the strength of connections between nodes of one layer and all nodes of the subsequent layer. In my case the weights are initialized with random values between -1 and 1, ensuring that the data fed into the network falls within a functional range of sigmoid (-4, 4). Biases determine how significant a node should be within the network by adding a flat value independent of the weights. biases are initialized to 0 allowing their influence on the network to evolve during the training process.

![WeightInitalize](https://github.com/MyutVoilim/AI-Digit-Recognition/assets/54462267/f2e704c9-3975-4ab6-bfc6-055b16443bea)

Why:
This method of initialization is critical for producing an effective environment for the neural network to learn and converge during training. Randomizing weight values breaks symmetry preventing neurons within the same layer from learning in an identical manner and instead allow for diversification.
Pitfalls and Considerations:
As mentioned already, initialization must follow the effective range and properties of the activation function used in conjunction with how how normalized data is added along with the weights and biases. Values to high and sigmoid will run into the vanishing gradient problem and hurt training performance.


Forward Pass and Training
With inputs normalized and values initialized, data can finally take a full pass through each layer of the network with weights and biases applied until the eventual output layer.  The values of that output layer are passed through the activation function sigmoid to give a 0 to 1 probability for each number, useful for classification. Their are many architectures for how layers should interact with each other but I opted for a more simplistic feed-forward neural network that does a single pass from one layer to the next.

![Forward Pass](https://github.com/MyutVoilim/AI-Digit-Recognition/assets/54462267/0494532a-30c8-4d52-aa42-62a75578e362)

In order to initiate training a methodology must be adopted. I utilized a variant of a Stochastic Gradient Descent (SGD) approach to compare and train the network on a per image basis. This process updates parameters using a gradient descent function on mini batches, in my case a batch of 1. This has the benefit of frequent adjustments and a straight forward implementation, although, there are some drawbacks that I’ll mention later. With the output calculated and the network’s guess compared to the expected values, it is sent for error correction. 

Error correction is achieved through three components, a loss function that determines the degree of error, backpropagation utilized to traverse backwards from the output layer up through all the hidden layers, and a gradient descent algorithm that updates the weights and biases with each update getting progressively smaller as it moves up the hidden layers during backpropagation. The length and rate for training is determined by the user through epoch count and learning rate. The learning rate has significant effects for the behavior of the convergence speed and landing at the minimum. Large rates causing quick but unstable convergence that may overshoot the minimum and fail to ever fully converge while smaller rates are slower and risk getting trapped in local minima.

	Their are several components of my approach that likely limit performance with several improvements to be made. Firstly, the use of a single image SGD approach may lead to over adjustments during error correction that the use of mini batches could help avoid. Single images also limit the computation efficiency that parallel processing of mini batches would have. My current approach with the training data is to run through the entire set once which could easily lead to overfitting. Adding variance to how training data is passed through along with additional augmentation such as slight rotations, translations, and noise to the images would allow for a more effective approach to new inputs. 

 ![Backpropagation](https://github.com/MyutVoilim/AI-Digit-Recognition/assets/54462267/70e91e2e-71d4-4be2-9fe0-bdb023ba949c)

When it comes to the error correction, my loss function is not a traditional approach such as the cross-entropy loss function. I also realized on analysis that the loss function used is actually only applied on the output layer when traditionally it would be calculated on a per layer basis. The system still seems to be effective for my application, however, it is only a partial loss function that could be improved. While I do use sigmoid to give output probabilities between 0 and 1, it may not be fully utilized, especially in the context of a cross-entropy loss function, since I do not apply a softmax distribution to the output layer. What this means is that the probability distribution of the system may equal greater than 1 with all outputs combined instead of being a classical probability total of 1. This could hurt the nature of how the system makes adjustments as well as make it difficult to work with other loss functions that rely on that foundation such as cross-entropy. The use of a static learning rate is also limiting as a more adaptive approach could allow for initially larger adjustments for quick convergence and later shift to a slower rate to complete convergence onto the minimum.

My main focus with this project was to take on a challenge while critically analyzing and documenting my process. Their were a million things I changed and adjusted while developing and, as mentioned throughout this documentation, a million more that I could have changed. There is a difficult line that must be drawn in software development for when to “complete” a project, especially in the context of something with limitless potential as a neural network. I the real world that Is usually dead lines and development power, in my case it was when to stop and analyze. What was most important was to try and understand as best I can the processes that make this neural network work and what pit falls and potential improvements could be made. 

	I think that the nature of how neural networks even come to “learn”  and retain information is incredibly interesting and problematic for making changes. It is not always straight forward how information is being created/stored and the effects that changes have on this process when making improvements. Their are decades of empirical evidence for effective methods of increasing performance and continuous attempts to understand the true nature of how a neural network understand a task. However, increasing the complexity of the architecture, methodologies, and scale of these systems creates a fuzzy black box of processes that work in ways we may not be able to comprehend, even if we claim we do. I learned so much in creating this from helping push my fundamentals to learning about complex systems, and most importantly my weaknesses and what I can strife to improve.
 
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

