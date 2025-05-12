# Wayfarer Encounter System: Complete Design Document

## Introduction

Wayfarer is a medieval life simulation game designed to create immersive, narratively rich encounters across a wide range of scenarios. Unlike traditional RPGs that use different systems for combat, social interaction, and exploration, Wayfarer employs a unified encounter system that handles all forms of interaction through the same core mechanics.

This document details the complete encounter system, explaining the motivation behind design decisions and providing concrete rules for implementation. The system aims to achieve the following goals:

1. **Mechanical Consistency**: Use the same underlying systems for all encounter types
2. **Strategic Depth**: Create meaningful choices that reward different character builds
3. **Narrative Integration**: Seamlessly blend mechanics with engaging storytelling
4. **Elegant Simplicity**: Focus on depth through interaction rather than complex rules
5. **Verisimilitude**: Create a system that feels natural and appropriate to the medieval setting

The core innovation of the Wayfarer system is its tag-based approach, where characters develop certain approaches to problems (HOW they tackle challenges) and focuses (WHAT they concentrate on). Different locations naturally favor certain approaches through strategic tags, while narrative tags create evolving constraints as player approaches intensify.

## Conclusion

The Wayfarer encounter system creates a universal framework for all interactions in the game, replacing traditional specialized systems with an elegant tag-based approach. Key strengths include:

1. **Unified Mechanics**: The same core systems handle social, intellectual, and physical encounters
2. **Strategic Depth**: Location strategic tags create meaningful choices about approach selection
3. **Character Archetypes**: Natural advantages for different character types in different situations
4. **Narrative Integration**: Mechanical systems are presented through appropriate narrative context
5. **Elegant Tag System**: Simple approach and focus tags interact to create complex gameplay
6. **Deterministic Design**: Clear, precise rules ensure consistent and predictable behavior

The system's design comes from its clear separation of responsibilities:
- **Base Layer**: Choices modify approach and focus tags
- **Encounter Layer**: Strategic and narrative tags determine mechanical effects
- **Presentation Layer**: The narrative adapts to encounter type

This architecture creates meaningful player agency and varied encounter experiences while maintaining consistent underlying mechanics.

The design prioritizes elegant simplicity over unnecessary complexity, creating depth through the interaction of simple systems rather than through complex individual mechanisms. This supports the game's goal of creating immersive, narratively rich encounters that allow players to develop distinct approaches to challenges in a medieval world.