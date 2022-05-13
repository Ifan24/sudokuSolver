# sudokuSolver
Use reinforcement learning to solve sudoku puzzle in unity 3D

# Actions
there are 13 discrete actions

0 - move up

1 - move down

2 - move left

3 - move right

4 - 12 click number 1-9 at the current position

# Rewards
- add -0.001 for every step.
- set -1.0 if the agent moves across the borders (or not have it to lower the difficulty) and end Episode
- add -0.001 if the agent tried to choose a number at the current position that is not available
- add -0.001 if the agent tried to choose a number at the current position, but there is already a number 
- set to -1.0 if the agent tried to choose a number at the current position, and it leads to an incomplete solution for the puzzle (during wave function collapse, some other grids have no available numbers left)  and end Episode
- add 1/(9*9-known_number) if the agent picks a number at the current position and the action does not have any bad effect like above
- set +1.0 if the agent solves the puzzle and end Episode

# Observations
- Current Agent position (2 ints)
- number left to choose (int)
### Game state:
for all 9*9 grids of the puzzle:
- does this grid has a chosen number? (bool)
- the chosen number of the grid (I use one-hot encoding, which generates 10 ints,  0 for not having a number)
- the numbers an agent can choose at this grid (9 bools)

stack of 2 observations for some short term memory


# Rant
After a few days of hyperparameter tuning, adjusting rewards, and observations, I have concluded that reinforcement learning in its simplest form is not able to learn to solve sudoku.
I believe the reason for that is that the game state is too large for the agent to understand how to play the game.

Sudoku has many levels of difficulty; the more known numbers, the easier the game.

I first tried to train the agent by randomly starting the game with a known number from 17 to 80, but I then realized it might be too hard as a starting point. 

Then I saw imitation learning in the ml-agents toolkit, which is perfect for the game as I can just solve the game using a solver I found online, and I can record the process and let the agent learn from it.
So I record the solver solving the game in the simplest setting (the known number is 80) and use the GAIL and behavioral cloning with high strength for the training. But the agent can still not learn from it even in the simplest setting!! After 2 hours of training, agents were still unable to solve the puzzle, and it would just move up and down and not try to generate a guess. Maybe there are some errors in how I set up the rewards, but I tend to believe that only deeper reinforcement learning algorithms like deep Q-learning can solve such puzzles. 
