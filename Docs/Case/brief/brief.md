# Game Developer Case 2026 — text transcript

Verbatim text of [game-developer-case-2026.pdf](game-developer-case-2026.pdf), extracted so agents can
read it without a PDF renderer. The PDF is the original; if the two differ, the PDF wins. Images in the
PDF are decorative and carry no requirements.

```text
=== PAGE 1
Game Developer
Case
=== PAGE 2
1.  Goal
Build a small vertical slice of Color Block Jam in Unity. 
The slice must contain a playable level flow and a level editor.
We evaluate code quality, architecture, and the usability of the editor. We do not evaluate art.
2.  Constraints
Unity version: 2022.3.62f2
Reference resolution: 1080 x 1920 (portrait)
Use the assets that we supply. Do not buy or import other art assets unless you think is absolutely necessary.
You can use any LLM tool. Tell us in the README which tools you used and for which parts.
Asset files: https://drive.google.com/drive/folders/1YqA5qE3C0GRtHIsxWQuelb2lsM3wsj6c?usp=sharing
3.  Deliverables
Send one link (Git repository or archive) that contains these items:
1.The Unity project. Include the Assets, Packages, and ProjectSettings folders. Do not include Library, Temp, or Build.
2.A README.md file. See section 7.
3.A build for Android (.apk) or a short screen recording (maximum 3 minutes) that shows the full flow.
=== PAGE 3
4.  Required scope
4.1 Home screen
Show a tab navigation bar. The tabs are placeholders. Do not implement the other sections.
Show the level button and the top UI.
The settings button must open the settings screen.
The other buttons must give visual feedback on touch. They do not need a function.
Acceptance criteria
The level button starts the current level.
The settings button opens the settings screen.
The layout is correct at 1080 x 1920 and stays correct on a 20:9 device.
4.2 Settings
All buttons are placeholders, but each button must give visual feedback.
The vibration, sound, and music toggles must show their on/off state.
Acceptance criteria
Each toggle shows a different visual state for on and for off.
The player can close the screen and return to the home screen.
=== PAGE 4
4.3 Gameplay
Implement the first 5 levels. Build the levels from the supplied assets.
The timer must function.
The coin inventory must function.
The fail popup must function.
Pause, restart, and home navigation must function.
Boosters do not need a function.
The life system can be a placeholder.
Acceptance criteria
The player can complete a level and continue to the next level.
The timer counts down and stops the level at zero.
The fail popup opens when the level fails. The popup gives a way to restart the level and a way to return to the
home screen.
Restart sets the level back to its start state. No data stays from the previous attempt.
The coin count stays correct after the player leaves the level and returns to it.
=== PAGE 5
4.4 Level editor
Build a level editor for non-technical users. The editor must control these items:
Board width and height
Door placement
Block placement
Timer value
Acceptance criteria
A designer can make a new level, save it, and play it. The designer does not write code and does not edit a file by
hand.
The editor shows the level in a visual form. It does not show only numeric fields.
The saved data has one format. The game reads the same format.
The 5 levels from section 4.3 come from this editor.

=== PAGE 6
5.  Optional scope
Complete the required scope first. Then, if you have time, add one or more of these items:
Level validation. The editor gives a warning before it saves an unsolvable level.
Functional boosters
Ice blocker
Arrow block
6.  Evaluation
We give the most weight to these points, in this order:
Architecture. Separation of data, logic, and view. Low coupling. The code is easy to extend.
Editor usability. A designer can use the editor without help.
Code quality. Clear names, no dead code, consistent style.
Correctness. The acceptance criteria in section 4 are satisfied.
Performance. No unnecessary allocation in the update loop. Stable frame rate on a mid-range device.
Scope control. A small and complete result is better than a large and incomplete result.
=== PAGE 7
7.  README content
Write a short README. Include these points:
How to open and run the project
How to open the editor and make a level
Your architecture decisions and the reason for each decision
Known problems and incomplete parts
The LLM tools that you used and the parts that you used them for
Your approximate work time
8. Notes
Follow industry best practices.
If you find a problem in the supplied assets, tell us. Feedback on the assets is welcome.
If a requirement is not clear, make a decision, continue, and write the decision in the README. You can also reach
out to our Talent Team via LinkedIn or email. 

=== PAGE 8
You have 7 days!
Good Luck!
```
