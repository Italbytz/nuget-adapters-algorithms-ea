# Introduction

The [Hexagonal Architecture](https://web.archive.org/web/20180822100852/http://alistair.cockburn.us/Hexagonal+architecture), also known as the Ports and Adapters pattern, is a design approach that emphasizes separation of concerns by isolating the core application logic from external systems like databases, user interfaces, or APIs. This is achieved through the use of "ports" (interfaces) and "adapters" (implementations), enabling easier testing, maintainability, and flexibility in swapping external dependencies without affecting the core logic.

This repository provides C# adapters for Evolutionary Algorithms inspired by [FrEAK](https://sourceforge.net/projects/freak427/). The NuGet package is called [Italbytz.Adapters.Algorithms.EA](https://www.nuget.org/packages/Italbytz.Adapters.Algorithms.EA) and offers a [docfx](https://italbytz.github.io/nuget-adapters-algorithms-ea/) page. Ports are in the NuGet package [Italbytz.Ports.Algorithms.EA](https://www.nuget.org/packages/Italbytz.Ports.Algorithms.EA) (Source: [nuget-ports-algorithms-ea](https://github.com/Italbytz/nuget-ports-algorithms-ea)).
 


# Getting Started

The project contains a set of unit tests demonstrating the use of the provided ports and adapters.