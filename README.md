# Introduction

The intent is to build myself a sandbox for learning and playing in. I wanted something that is reflective of real-world problems. This is very loosely based on a product, or products I have worked on at various points in my career.

This is to give me somewhere I can:

* Experiment with libraries and technologies other than the ones we would use by default e.g.
    * xUnit instead of NUnit
    * Fluent Assertions instead of Shouldly
    * Fluent Validation instead of Data Annotations
    * Serilog instead of log4net
* Flesh out things I thought were a bit under-developed
* Explore potential solutions to sources of friction or irritation
* Experiment with new technologies
* Experiment with different architectural solutions
* Experiment with different ways of arranging code

# Mono-repo FTW!

For years any training or testing I've done has been in relatively simple projects or solutions. Well, that works to some degree, but it is hard to explore things beyond the trivial. If I wanted to explore things in more depth I would need something a bit more detailed. A mono-repo seemed like a good strategy for this sort of sandbox.

I'd found in professional environments with small teams having things in separate repos didn't work as well in practice as the theory would suggest; they could often gum up the flow of development. You'd build your library code, then wait for the pull request to be approved, then wait for the NuGet package to build, then add it into your web API, then wait for the pull request for _that_ to be approved, then wait for that to build, and then _finally_ be able to consume that from a UI. Separate repos are great for stable products and code bases, and they do have a _lot_ of advantages. They're not quite as good when you're working on rapidly changing code though IMO, and a code base for learning and experimenting is going to be changing _very_ rapidly!

A mono-repo could still be structured to keep separation between projects, but without all the overhead and context switching, yet still provide me a sandbox to test out what I wanted, when I wanted.

# What's In A Name?

As you may have guessed DGT is an acronym. It stands for Decaf Genre Terror 😁 So what's going on there then?

For years I've gone by the handle of "mmmcaffeine". I, like many people and software engineers in particular, have a love of coffee. It was also partly in homage to Homer Simpson and his "Mmm donuts" catchphrase. That's the first part of the explanation...

Throughout the majority of my career I've had a love of unit testing, largely because it facilitates refactoring. I've tried to live with the philosophies of "red, green, refactor" and "make it work, make it right, make it fast". That's the second part of the explanation...

Now, the original definition of refactoring is "the process of changing a software system in such a way that it does not alter the external behaviour of the code, yet improves the internal structure." (from the [seminal work by Martin Fowler](https://martinfowler.com/books/refactoring.html)). For a lot of people it is used in a looser fashion to mean something more akin to "re-arrangin stuff". And if you re-arrange the letters of "red, green, refactor" one of the things you can get is "Decaf Genre Terror". That struck me as perfect; as someone whose development is fuelled by coffee the genre of decaf fills me with terror! ☕😱