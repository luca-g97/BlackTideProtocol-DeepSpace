# Black Tide Protocol

*A multiuser interactive fluid-simulation experience designed for the Ars Electronica Center’s Deep Space 8K.*

> A defective oil platform spews a deadly, colorful tide into the ocean, threatening to wipe out an entire seal colony. Pilot skimmer ships with your body, track down absorbable oil particles, and team up to split complex oil mixtures before removing them. Can you stop the black tide before it's too late?

<video src="https://github.com/luca-g97/BlackTideProtocol-DeepSpace/raw/refs/heads/main/BTP-Documentation.mp4" autoplay loop muted playsinline width="100%"></video>

**Creators:** Luca Geiger (DE), Dino Ponjevic (AT), Alexander Hödlmoser (AT)  
**Context:** Developed in the Interactive Media course *Game Spaces* at [FH Hagenberg](https://www.fh-ooe.at/campus-hagenberg/).  
**Credits:** Built upon the open-source [fluid simulation by Sebastian Lague](https://github.com/SebLague/Fluid-Sim), heavily customized for high-performance, cooperative gameplay.

---

## ✨ Key Features

* **Physical Interaction:** Tracked entirely via the Pharus system. Your physical position controls your skimmer ship. 
* **Dual-Screen Fluid Dynamics:** Over 100,000 particles simulated in real-time. Floor and wall projections run interconnected physics with independent gravity to simulate a realistic, room-scale ocean.
* **Cooperative Color Mixing:** In *Color Mixing Mode*, players are assigned primary colors. To clean up secondary-colored oil (Orange, Violet, Green), players must physically group up to create a glowing tow rope and mix their colors.
* **Dynamic Crowd Scaling:** The difficulty, ocean currents, and available colors adapt in real-time based on the number of people in the room (scaling from 1 to a theoretical 1,024 players).

---

## 🛠️ Quick Technical Overview

The core simulation logic is driven by highly optimized Compute Shaders. To maintain visual consistency without breaking the physics, the rendered visual colors of barrels and particles are entirely decoupled from their underlying data.

* **Core Simulation Code:** Located in `...\Assets\Scripts\Sim2D` (Divided into `Wall` and `Floor` behaviors).
* **Tracking:** Designed for Pharus data. If no players are detected, the simulation automatically pauses.

---

## ⚙️ Configuration

The game is highly modular and designed to be adjusted on the fly for different museum crowds or event runtimes. All major parameters are exposed via XML files located in `...\Assets\StreamingAssets\`. 

*(Ensure `<useXML>true</useXML>` is enabled in your config to apply these).*

* **`SimulationSettings.xml`**: Adjust screen bounds, FPS, gravity, max particles, and current strength.
* **`DifficultySettings.xml`**: Tweak how animal health and hazards scale with player count.
* **`MissionSettings.xml`**: Define round length, penalty scores, and restart delays.

---

## 📖 Full Documentation

For an in-depth breakdown of game progression, detailed color-array logic, Pharus edge cases, and a comprehensive list of all XML parameters, please refer to the attached **[Documentation PDF](https://github.com/luca-g97/BlackTideProtocol-DeepSpace/blob/main/BTP-Documentation.pdf)** included in this repository.
