# Sword Mod

Adds a fun sword item to Rain World. This new weapon is fairly powerful, but not so powerful that it makes the game stupidly easy.

While this code is a bit messy, it's still an okay example of adding a Rain World object. HOWEVER, if anyone wants to add an object of his own to Rain World, I have one simple recommendation:
# USE FISOBS
I ended up manually doing everything that Fisobs does, which was simply a waste of time.

The sword works in an unusual way: It is a weapon object (like a rock or spear) so it gets THROWN just like a rock. The sword then gets continually repositioned in front of the player until its swing timer finishes; at which point, the creature who "threw" it attempts to grab it.
