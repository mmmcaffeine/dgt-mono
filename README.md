# Introduction

The idea of this repo is to build myself a sandbox for learning and playing in. I wanted something that is reflective of real-world problems. Initially the intent is for this to be very loosely based on products I have worked on at various points in my career (without infringing on any IP of course!)

This is to give me somewhere I can:

* Experiment with alternatives to the libraries and technologies I was typically using in the workplace e.g.
    * xUnit instead of NUnit
    * Fluent Assertions instead of Shouldly
    * Fluent Validation instead of Data Annotations
    * Serilog instead of log4net
* Explore potential solutions to sources of friction or irritation
* Flesh out things I thought were a bit under-developed
* Experiment with different architectural solutions
* Experiment with different ways of arranging code
* Experiment with new technologies

# Mono-repo FTW!

For years any training or experimenting I've done has been in relatively simple projects or solutions. Well, that works to some degree but it is hard to explore things beyond the trivial. If I wanted to investigate things in more depth I would need something a bit more detailed. A mono-repo seemed like a good strategy for this sort of sandbox.

As a professional I've typically worked in small teams with a broad range of responsibilities. In those scenarios I found having separate repos for things didn't work as well in practice as the theory would suggest; they could often gum up the flow of development. You'd build your library code, then wait for the pull request to be approved, then wait for the NuGet package to build, then add it into your web API, then wait for the pull request for _that_ to be approved, then wait for that to build, and then _finally_ be able to consume that from a UI. That could be more than a little frustrating. Separate repos are great for relatively stable products and code bases, and they do have a _lot_ of advantages. They're not quite as good when you're working on rapidly changing code though, and a code base for learning and experimenting is going to be changing _very_ rapidly!

A mono-repo could still be structured to keep appropriate separation between projects but without all the overhead and context switching, yet still provide me a sandbox to test out what I wanted, when I wanted. If a mono-repo can work on the sort of scale that Google and Microsoft have to contend with it can certainly be made to work at the sort of scale a little playground would need!

# What's In A Name?

As you may have guessed DGT is an acronym. It stands for Decaf Genre Terror 😁 So what's going on there then?

For years I've gone by the handle of "mmmcaffeine". I, like many people and software engineers in particular, have a love of coffee. It was also partly in homage to Homer Simpson and his "mmm donuts" catchphrase. That's the first part of the explanation...

Throughout the majority of my career I've had a love of unit testing, in no small part because it facilitates confident refactoring. I've tried to live with the philosophies of "red, green, refactor" and "make it work, make it right, make it fast". That's the second part of the explanation...

Now, the original definition of refactoring is "the process of changing a software system in such a way that it does not alter the external behaviour of the code, yet improves the internal structure." (from the [seminal work by Martin Fowler](https://martinfowler.com/books/refactoring.html)). For a lot of people it is used in a much looser fashion to mean something more akin to "re-arranging stuff". If you re-arrange the letters of "red, green, refactor" one of the things you make is "Decaf Genre Terror". That struck me as perfect; as someone whose development is fuelled by coffee the genre of decaf fills me with terror! ☕😱