Simulated Immersion, Part 3: Product

Part 1 was about the legacy of immersive sim games and how they pushed systemic game design to the max. Part 2 dealt with the game design takeaways from that legacy, and things you can consider if you want to walk in its footsteps. (Please note that there’s a silent in my opinion added on top of all this. If you disagree at any point, please do so in comments or to annander@gmail.com.)

That’s all fine. But let’s get down to the interesting stuff: what’s the product like and how can you approach making one?

For just a moment, stop to ask yourself: what do thieves do?

Maybe your answer is that thieves steal things. That they break into houses where they’re not supposed to be, interact with shady characters like fences and smugglers, pick pockets, rob graves, snatch jewelry, and partake in other unwanted activities. The act of stealing things is a time-honored human tradition and one that has more variation than you may first realise.

Naturally, all of these things and more are present in the Thief games. Particularly the second installation explored thieving in some depth. Not all fans agree that this made it better, of course. As you probably know, gamers can’t agree on anything, and imsim fans are no exception.

But to summarize the gameplay mechanics of Thief for the sake of argument, they are about stealing things and getting away with it. The whole game’s framework is built around this fairly simple premise. It didn’t start out this way, so the strong and simple premise seems to have come out of a long and painfully crunchy creative process.

“I think it was Paul who just stuck his head in the door and said, ‘why not just call it Thief?’ And at that moment it was like a lightbulb went on, and it gave the team a really solid direction. One of the most powerful things you can do–and I admit I’m very bad at this–is to name a game something that instantly tells players exactly what they’re going to do.”

Warren Spector, The Story of Thief & Looking Glass Studios
Finding an identity like this is incredibly powerful, and there are a few ways you can look at the player’s mental model and game avatar as ways to define how your own systemic game is constructed. Which features you implement and how. Even who your character should be. Maybe most importantly: it’s a great a way to inform your team what they are working on and how to make the most of it.

If the name of the game is Thief, stealing things and trying to get away with it makes intuitive sense. Everything else–avoiding guard patrols, passing loot off to a fence, etc.–comes for free.

Core Idea
If thieves steal things and try to get away with it, what would be the snappy single-sentence core idea of a game called Paladin, War Correspondent, Watchmaker, Nurse, or Accountant?

I bet you had ideas right away after reading those titles, with more or less enthusiasm depending on how exciting your everyday life is. Giving things a name can be a powerful way to mine them for mechanics.

I’ll give you a concrete example from my own past design explorations. It’s for a game that ultimately never got made.

Like every stealth game after it, Thief motivates you to risk detection and then gives you tools to remain undetected if you manage to use them in a skillfull manner. In interviews, it’s been referred to as “active” stealth. How guards talk, which tools you have available, where loot is located, how patrols are setup, even the storytelling: everything in the game comes back to this central mechanic and is used to reinforce or challenge said mechanic.

So starting from there: what’s the core idea of your systemic game? The one thing you build your whole game around?

The idea I had, and will use as an example, was “first-person movement in zero gravity.” Before you mention all the games that have done this, you must understand that this prototype was worked on in 2007. Before Shattered Horizon came out and before VR made zero gravity a nice way to move without continuous input.

First-person zero gravity will be the basis for the rest of this example. It’s not as good as being a thief, since it doesn’t imply an identity, but it’ll do.

Loop Mechanics
Coming up with the control scheme was fairly straightforward. It just used the standard Left Stick/WASD setup for first-person shooters. Q and E would lean out when you were attached to a surface and would rotate you on the camera’s forward axis when in freefall. It all felt pretty intuitive, even to people who tried the earliest version of the prototype. (This prototyping was a huge part of how I learned to program anything in C++.)

Watching hours of astronaut footage, there seemed to be four main ways to move in zero gravity:

Attaching. Magnetic boots, velcro, suction cups–something. Mimics how you walk on the ground when there’s gravity, except only on a viable surface. Think Super Mario Galaxy.
Launching. Touching a surface or handhold, you push away much as you may push into the water from the side of a swimming pool. This style of zero gravity can also be found in Dead Space. You launch from one surface to another.
Bracing. Using a foot, hand, or other part of the body to stabilise against the structure of the station itself. Often using specially placed hand- or toe-holds. In astronaut footage, considering the cramped ISS, this is the standard way to move. Bracing can also be partial, where a hand or foot is simply used to stabilise a rotating person and avoid nausea.
Freefall. Complete freefall movement mostly occurs for real astronauts when they are on a spacewalk. It’s much like launching yourself off a surface except that you also have some kind of equipment capable of changing your direction. Theoretically, you could “swim” if the air mixture was thick enough, but for clarity it’s easier to use a jet or other instrument. Think of the scene in Wall-E where a fire extinguisher is used in space–that’s exactly it.
For attaching, magnetic surfaces made sense. Most of your environments will be artificially constructed (probably) and metallic surfaces are easy to recognise. But that also led to other ideas about surfaces. How about especially bouncy surfaces, surfaces that cushion you if you move really fast, and of course breakable surfaces that you can go straight through if you’re not careful?

There are also things like handrails, both attached to solid superstructure and to objects like containers that are free-falling themselves. Many of the movement scenarios that came up felt almost like a dance, where you could time launching from one handhold to another while ducking under crates and containers.

From the book Ender’s Game, another idea that came up was to use a grapple–a magnetic one, of course–that would allow you to brake, switch directions mid-launch, and perform other neat stunts based on accumulated velocity. For example, go really fast down a hallway, grapple the wall, swing into a doorway, and then keep your momentum going through the turn by detaching the grapple at the right moment.

There you have it: a first-person zero gravity movement core.

Fail States
This leads us to the next stage in the comparison. Where Thief implies that you want to get away with stealing, space and zero gravity has other dangers than getting caught. Most of them quite fatal. There could be a layer of getting caught here too, if we want a more character-driven experience, but for now we’re focusing on the activity of zero gravity and the various fatal misadventures that may occur.

First of all, space is extremely inhospitable and humans die from exposure to vacuum in a much shorter time than you’d want to know. They also need to breathe, eat, shit, and sleep, three of which tend to be modelled by video games. Oh, and humans prefer to not bleed to death or die in other violent ways either.

Suffocation happens if life support breaks down or you run out of oxygen in an oxygenless environment. Or if you go swimming in the water tank for some reason.
Injury of every type can happen–including fatal collisions with solid objects–simply because you can easily gain a high velocity before crashing into a wall, or end up crushed flat between containers gliding through your space. Or cut to ribbons by shrapnel from an explosion. Or lasers. Or swords. Or laser swords.
Vacuum Exposure will boil the water in your body and cause your lungs to expand in a highly unhealthy manner. Loss of consciousness followed by death. This doesn’t have to be through explosive decompression. It can also be the result of a punctured space suit or other gradual atmospheric leaks.
Freefall can also be incredibly disorienting, and you can roll and tumble in ways that can be nauseating and have you bounce off into infinity if you’re not careful (or tethered).
The assumption by now is that environments will be artificial. Space stations orbiting distant colonies, perhaps, or starships plying the absurdly distant trade lanes. It’s part of a narrative context that doesn’t exist yet. But what we do know is that artificial environments come with a range of additional hazards.

Radiation Poisoning is dangerous, and the unmistakable Hollywood-sound of Geiger counters have been a mainstay in video games since at least Half-Life. Avoid it, or cough your lungs out.
Chemical and Electrical Burns are prevalent around starships and space stations, and may become obstacles to your freefall shenanigans.
Security Systems are another trope, whether cameras or tripwires or merely locked doors that require colored keycards.
Magnetism in the wrong places may cause your magnetic grapples and boots to misalign or even pull you away from where you really wanted to go.
Other People can be a problem. Whether zombies or other classic space enemies, robots, or merely the people who live on the space ship; there is no end to how many ways people can cause problems.
Reward Structures
Alright. Now we know how you die and what you need to avoid. But that can’t be all there is to it, can there? In Thief, Garret at least gets to collect coin and loot along the way. He has a reason to risk getting caught.

Just going forward with the inspiration from “first-person zero gravity” we already know some things that the player will want to look for:

First of all, oxygen. This can be a constant search for precious O2 in a survival horror manner, or it can be something you stockpile and keep track of gradually. In either case, it’ll be something you keep an eye out for.
Handrails placed in the environment will effectively breadcrumb your way ahead. Though freefall movement is nice and all, it’s also more dangerous than jumping between handrails.
Whether in the form of abstract packs or more specific medicines and bandages, health will let you bind wounds, treat injuries, and stay in shape. It can also go in the other direction, where you don’t really patch yourself up but rather find boosts to increase your speed, recovery, etc.
If the magnetic grapple is a resource, the hook will be something you will be looking for. If it can be detached and reattached, you need fewer of them than if they are expended on use.
Keys to unlock doors can take any different shape. From something like the vent tool in The Chronicles of Riddick: Escape From Butcher Bay, to the classic red and blue keycards in DOOM, or just passcodes or DNA tags or whatever you want. Just remember to put the door before the key so that the player knows that it matters.
If we added a narrative context, then logbooks, journals, potential loot tied to whoever you are and what you want, and so on; all of that can be added also. It’s just that so far we’ve only used the gameplay to inform our decisions.

Rules
We’ve already mentioned a few rules that have systemic significance. Magnets and magnetic surfaces, for example. Tethers and momentum. Breakable surfaces. Explosive decompression and leaking atmosphere.

Exactly which rules we’d settle on if we made this game properly is of course a matter of iterating and testing. What’s important is merely to define rules in a way that’s easily understood and can be effectively communicated.

For clarity, let’s summarize some rules here:

Magnets can attach to magnetic surfaces. (Used for magnetic grapples, magnetic boots, crates with magnetic clamps, and other variants you can think of.)
High velocity impacts crush breakable surfaces. (Can be used by the player gaining enough velocity by moving or pushing other objects.)
Tethers maintain momentum through turns. (For those Ender’s Game-style rapid turns around corners.)
etc.
Conclusion
Another addition could be combat. If you’re shooting and fighting while moving in zero gravity, that adds a whole other layer of rewards. New weapons. Ammunition. Weapon selection based on whether a weapon might penetrate the wall and risk life support or lockdown. Outmaneuvering enemies. There’s many different ways this can be expanded, which is really the main point being made: if you start from a simple easily repeated mechanic (like “first-person zero gravity”), you can design the rest of the systems defining your game around it.

Authorship vs Emergence
“Looking Glass games weren’t driven by a singular 90s auteur. In fact, the very absence of ego in the studio’s culture meant its many ‘bright stars’ were happy to adhere to a shared vision.”

Randy Smith
Late film critic Roger Ebert received the ire of many gamers and game developers after saying games can never be art. He said, “Video games by their nature require player choices, which is the opposite of the strategy of serious film and literature, which requires authorial control.”

We’ve had many games, before and since, that sign up fully to Roger Ebert’s views on the merits of authorship. Building up to the airing of HBO’s The Last of Us adaptation, Neil Druckmann mirrored Ebert’s opinion. He said, “If the player can jump in and be, like, ‘No, you’re gonna make this choice,’ I’m, like, ‘Now we kind of broke that character.’”

Druckmann seems to be the kind of “90s auteur” that Randy Smith talked about. Focus on a strong creative vision, cinematic production values; script writing. Just like in film. We will have Druckmann represent the leftmost side of a single-axis scale between Authorship and Emergence.


On the other side of the scale, we find developers and designers who are more interested in creating experiences. Designers whose lifeblood is to let players make interesting choices–sometimes in ways the developers or designers never expected. This is often called Emergence, because the behaviors and experiences are emerging from the player’s interactions with the game’s systems. The types of choices that Ebert and Druckmann don’t want are everything to that line of thinking about and designing games.

This doesn’t have to be mutually exclusive. Games like Red Dead Redemption 2, with plenty of systems and emergence, still have authored stories that the player observes more passively. Druckmann’s own The Last of Us has systems for stealth and combat where the player is allowed to make many interesting choices, just not on a personal level for Joel or Ellie.

Let’s hear how Warren Spector sums this up, for game designers, from his Sweden Game Conference keynote a few years ago:

“It’s not about how clever and creative you are as a designer—it’s about how clever and creative players can be in interacting with the game world, the problems, and the situations you create. If you’re a game designer, and you think it’s about you, just leave now,” he said.

Due to this championing of emergence, we’ll use Warren Spector as the face for the right side of the scale.


The very different perspectives of Neil Druckmann and Warren Spector, illustrated as a sliding scale.
Both authored and emergent experiences can be immersive, so this is not to tell you that you must strive for emergence. But for the classic immersive sims, and most of their legacy, emergence has been extremely important. Served with a good story, absolutely, but using storytelling suited to games. Sometimes experimentally.

Personally, I’d argue that the higher tendencies towards authorship in games like Dishonored changes the dynamics of the gameplay from the classics to such an extent that it becomes a different kind of experience. Still love those games, but they are not quite the same as the classics. They are farther to the left on the above scale than something like the original Deus Ex, or the first two Thief games.

What I hope your takeaway will be is this—learn to let go of your authorial control and embrace that very thing Ebert and Druckmann protest against. More emergence means you come closer to the design paradigm of the original immersive sims. More authorship means you are making something more cinematic. Decide which one you are aiming for before you start working on your project.

Systemic Development Dangers
Emergent games based on a simple core with clever and easily communicated rules. Sounds awesome—if you’re into this sort of thing. But there are many dangers with systemic game development and some of the dangers are inherent to the developers as designers paradigm described in Part 2.

I bet you’ll recognise them.

Tunnel Vision
For a certain kind of developer, system development is merited by itself. It’s highly engaging to see systems connect and become a whole bigger than the sum, and to crawl deeper and deeper down the rabbit hole that is technology.

Poor Discovery
Developers are often subconsciously nice to the systems they work on. They play it how it works rather than how future players may play them. Many game designers need to be told that their balancing is off or that a certain encounter breaks if the player sprints into the room rather than walking.

This is really bad for systemic games, because the systems are supposed to be tinkered with.

Fun When Done
Sometimes a system simply sucks and there’s no nicer way to say it. It doesn’t do what it’s supposed to or does it in a way that fails to connect with anything else in a meaningful way. This can sometimes lead to a quite common handwaving of real problems: “it’ll be fun when it’s done.”

This line of thinking is common in game development and not always without merit. You know that some things will simply have to get their content in place before they are fun to play. The particles, assets, sound effects, multiplayer hosting, and so on. But with systemic development, this is rarely true.

If someone says “it’ll be fun when it’s done” when working on a system while the system shows no promise in the playable build, it’s better to scrap it and move on.

Hardcoded Exceptions
It’s tempting, particularly when you have short deadlines or deliveries to external stakeholders, to hardcode things instead of relying on a more data-driven approach. You take your precious systems and shoehorn them into a mold with the intention of impressing someone. Sometimes it’s merely impatience–a desire to reach the goal of getting to use the system faster.

Try to avoid this temptation and try to avoid having to make timed deliverables. To paraphrase a friend: it may take you six months to make the first piece of content using your system, but then you can make 100 more in a day. Systems are strong because of their systemic nature and the moment you start making hardwired content that overrides their unpredictable nature you will lose the magic.

A Systemic Development Process
This is getting long! But I might as well write about the whole systemic development process. This is something that has been spinning around in my head for probably a decade and something that has been demonstrated in some form or another multiple times.

1. Throwaway Prototyping
To communicate ideas in a tangible form and try things out before they are actually built, you make throwaway prototypes. Preferably in minutes or hours, not even days. They’re built to be thrown away after all–it’s in the headline. Pick something you want to illustrate and then take every possible shortcut along the way.

2. Proofing and Tooling
Emergence comes from the player’s interaction with multiple systems. Proofing gets you to the goal faster. You take an isolated system, say Character Actions, and you build them to a testable good enough state. While doing this you add rudimentary tools on top of them to enable designers, production artists, and others, to populate the proof with data.

What you end up with is a proof that can be tested and evaluated. If it feels interesting at this stage, you know you are on to something. Doesn’t matter if all you have is some programmer art and two test cases–if you can already see how it can potentially hook into other systems, it’s a successful proof.

Once a proof is done, however, you should move on to the next proof. Stay on each system for exactly as long as is needed to proof it and no longer. Your goal is to proof all of the project’s systems before you start improving what you have.

3. Facts, One-Pagers, and Improvement Planning
In reality, this isn’t a step-by-step process. You should spend lots of time doing 1 and 2. (If the financial gods smile upon you, you should spend half your project doing 1 and 2.) Along the way, you will need to define both what you have agreed on through the proofing process and how you want to expand on the systems once you reach production.

Facts are single-sentence descriptions of things you have all agreed on. “The player can carry up to three weapons” is a potential fact. Note these as they come up, but do make sure to note them.
One-Pagers are single pages with bullet point lists describing specific systems or parts of systems. If you have a Weapons one-pager, for example, it should tell you all you need to know about weapons in general, while the Shotgun one-pager specifies the details that only matter for the shotgun. Pictures, lots of empty space, and bullet point lists. Use Miro or similar tool, or a physical whiteboard.
Improvement Planning is where you make lists of all the things you should, could, and would do to improve on the proofs you have made. With that Character Action system you have, maybe you’d want to add some visual flair, animations on character hands, and functionality for looking at targets or something. All are things you know you could do in your engine, so put them in the improvement plan for the Character Action system. But you don’t do them now.
4. Merge Checkpoints
This step is specifically aimed at removing assumptions, supposition, and tunnel vision. You take some or all of the proofs you currently have and you put them together. Take the tools for a spin to see how they are to work with.

Every time you do this, you will find more facts, write more one-pagers, and probably add to the improvement list for each system. That’s perfect. But more than that, you’ll see the product for what it currently is. Just make sure to keep proofing if you’re not done yet, and don’t turn the merge checkpoint into production. That comes later.

5. Creative Direction
After a few merge checkpoints, you’ll finally see what the game is about. With the proofs proofed and the one-pagers one-paged, you need to start thinking more holistically. Ideally, the holistic direction has been present all along in a hands-on way, but now is when you really need to step it up and own the product you are making.

Art Direction. Lighting, colors, shape language, post effects, costume design; everything needs to be carefully thought through. Not only from an aesthetic point of view, but also from a systemic one. If the procedurally generated terrain looks weird, some paintovers may be in order to inform the programmers how the result should look like. The player needs guidance, and the rules and systems need clarity. All of this is aided by good art direction.
Design Direction. Player actions and intentions. First-time user experience. Accessibility. Testing for discovery, making sure that the weird and strange things players will do with your systems is actually supported. But also testing for bugs, compliance, and doing so with everything from on-staff QA testers to external focus testers. Play, play again, then play some more, and make sure to make the experience just a little better after each time you play.
Narrative Design. You need continuity and motivation, but you also need answers to the 5W+H that the player can immediately understand. One of the most crucial things to achieve for a systemic game is that the player can think of their own solutions and have their own ideas. You shouldn’t rely on explicit instructions. Find a way to make this possible, whether through the identity of the player’s avatar, or how the world reacts to that avatar. Make it make sense, then let the player have fun with it.
Technical Direction? As you can probably tell, systemic games are technical from day one. With all the prototyping and proofing going on, technical direction can’t wait until the fifth step in some list in a blog post–it has to be there all the time. This step is a good time to go through all of it and trim the fat, however. Refactor code, revisit half-baked systems and decide what to do with them, refine the tools, optimize performance, and of course executing on the plans of the other directors.
6. Content Production
After you’ve passed the half-point of your project’s duration, you reach this stage. Your systems have been proofed, your tools are in place, and you have spent some time coming up with a solid direction. Now it’s time to make the game!

Now you bring out your agile processes and whatever else you fancy, and you go to town. Go through the direction, set up the improvement planning as actual tasks, make sure to follow the facts, and check so the one-pagers are implemented or ready to be so. Use all the tools you’ve made and polished, and have fun delivering your game!