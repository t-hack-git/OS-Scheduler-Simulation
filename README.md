# OS Scheduler Simulation
Description
This was started as a class assignment, and I really like how it turned out. The progrm simulates two operating system scheduling algorithms and compares their running times. First-Come-First-Served (FCFS) is used for comparison, while the Multilevel Feedback Queue (MLFQ) implementation took the most code planning.

This MLFQ algorithm uses three queues (preemptive - absolute priority in higher queues) and the following requirements:

Queue 1 uses round-robin scheduling with a time quantum of 7
Queue 2 uses round-robin scheduling with a time quantum of 14
Queue 3 uses FCFS scheduling
All process start in Q1
If time quantum expires before a process's CPU burst, it is downgraded one level
Processes are not downgraded if preempted by a higher level process
Once downgraded, a process will never be upgraded
For the assignment, we were given 9 processes, each with a series of CPU burst and I/O times. My implementation sends these process through a FCFS scheduling simulation, then through the MLFQ simulation. For simplicity, both simulations are run back-to-back in the same program.

To do this, I used four class: Process to keep track of each process's data, Scheduling - parent scheduling class, FCFSSimulation, and MLFQSimulation. The StartScheduling() function of each simulation is a while loop where each iteration is equal to one CPU clock time, which is equal to one "CPU burst" for the processes. I use a hand-coded a linked list for round robin scheduling and a C# generic List class to hold completed processes. I first completed the simpler FCFS algorithm, then added the preemptive qualities of MLFQ - which was the most fun part of the assignment.

Since each simulation was over 700 CPU clock cycles, we were to simplify the output by only displaying the status of processes during each context switch - the event where the scheduler terminates or preempts a running process and schedules the next. At each context switch I show following data:

The current CPU time
Current running process
Processes in the Ready Queue with:
Next CPU burst time
Current priority level
Processes in I/O and remaing time in I/O
By following the order of each context switch, a Gantt chart can be created to see the order of which each process is scheduled. At the end of each simulation, a summary is also given to show the Response Time, Wait Time, and Turnaround Time of each of the nine processes, along with the simulation averages of each. Finally, the CPUThe utiliztion of each algorithm is displayed. My simulation proved FCFS superior for the given processes with 94.66% CPU utliztion, compaired to MLFQ's 89.75%. The chart to the right shows the data from my simulation next to a given data set for a Shortest Job First (SJF) algorithm simulation on the same set of nine processes. Click here for the full output file. And don't forget to click the code picture above to see the full source code!
