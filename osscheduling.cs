using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CPU_FCFS
{
    public class Process                            //PROCESS CLASS     
    {
        //Field
        public int arrivalT;
        public int waitingT;
        public int? responseT = null;                            //make nullable to ensure calculation only once
        public int finishT;
        public int iCPU;	                                 //index of cpu burst
        public int iIO;		                                 //index of io time
        public int name;
        public int Q = 1;                                        //Priority Queue for MLFQ only - all start in Q1
        public bool inIO;	                                 //true if currently in io

        public Process next;

        public int[] CPUBursts;
        public int[] IOTimes;

        //Methods
        public Process(int[] Bursts, int[] IO, int n)   //Constructor                                                    
        {
            CPUBursts = Bursts;
            IOTimes = IO;
            name = n;
            next = null;
            arrivalT = waitingT = iCPU = iIO = 0;
            inIO = false;
            Q = 1;
        }
        public override string ToString()               //Return Process#                                                      
        {
            return "P" + name;
        }
    }

    public class FCFSSimulation : Scheduling            //FCFS FUNCTIONS    
    {
        //Methods
        public FCFSSimulation()                                                //Constructor                                                        
        {
            front = null;
        }
        public void StartScheduling()                                         //Main loop through CPU time                                         
	{
		while( front != null || running != null )		      //Run until ready queue is empty
		{
			if( burst <= 0 && !front.inIO )
			{
				ContextSwitch();
			}
			
			--burst;  	                                   //Decrement burst of running process
			++currentT;	                                   //Increment current CPU time unit		
			UpdateIO();	                                   //Decrement io time for processes currently in IO

                	if (burst == 0) { EnterIO(running); }              //Current process Enter IO if burst is over
                	if (burst < 0) { ++CPUIdleT; }	                   //For CPU utilization (time all processes are in IO)
		}
	}
        public void ContextSwitch()                                        //Schedule next process and display ready queue and IO processes     
        {
            running = ScheduleProcess(); 	                           //Schedule next process (and remove from rq)
            burst = running.CPUBursts[running.iCPU];

            Console.WriteLine("Current CPU Time: {0}", currentT);
            Console.WriteLine("Now Running: {0}", running.ToString());

            PrintReadyQueue();
            PrintIOList();
        }
        public void EnterIO(Process p)                                      //Send Process to IO                                                 
        {
            ++p.iCPU;                                                       //prepare next CPU burst iterator

            if (p.iCPU <= p.CPUBursts.Length - 1)
            {
                p.inIO = true; 					            //Enter IO state
                p.arrivalT = currentT + p.IOTimes[p.iIO];                   //prepare Arrival for after IO
                ToReadyQueue(p); 				            //back to ready queue after I/O time
            }
            else
            {
                p.finishT = currentT;               
                Completed.Add(p);                                           //Add to final list if no more bursts
            }

            running = null;
        }
    }

    public class MLFQSimulation : Scheduling
    {
	    //Field
	    public int TQ;      					//Time Quantum

	    //Methods
	    public MLFQSimulation()                                     //Constructor           
	    {
		    front = null;
	    }   
	    public void StartScheduling()                              //MLFQ Scheduling       
	    {
		    while( front != null || running != null )
		    {
			    if( burst <= 0 || (TQ <= 0 && running.Q < 3) || Preempt() )      //if burst is 0 OR Time quantum is 0 and not Q3 OR preempted by higher priority
			    {
                    		if (front != null && !front.inIO)
                    		{
		                        ContextSwitch();
                	    	}
			    }
		
			    --burst;																						    //Decrement burst of running process
                	    if (running != null) { --running.CPUBursts[running.iCPU]; }     //this is done again here because of preemption (easiest place to update remaining CPU time)
			    --TQ;																						    //Decrement current Time Quantum
			    ++currentT;						            //Increment CPU time
			    UpdateIO();							    //Decrement IO times for processes in IO

			    if( burst == 0 ) { EnterIO(running); }			    //Current process enter IO if burst is over
			    if( burst < 0 ) { ++CPUIdleT; }			            //For CPU utiliztion (time all processes are in IO)
	            }
	    }       
	    public void ContextSwitch()                                 //MLFQ ContextSwitch    
	    {
            	if (running != null)
	       	{
                	if (running.Q < 3 && TQ == 0 && burst > 0)	                                //TQ is over before burst for Q1 or Q2
	                {
        	            running.arrivalT = currentT;            		                        //Update arrival time
                	    ++running.Q;                            		                        //Lower priority queue
	                    ToReadyQueue(running);                 	                                //back to ready queue
        		}
                	else if (running.Q == 2 && TQ > 0 && burst > 0 || running.Q == 3 && burst > 0)  //Q2 or Q3 preempted
	                {	
                    		running.arrivalT = currentT;              //Update arrival time
	                        ToReadyQueue(running);                    //back to ready queue
            	    	}
	        }

		running = ScheduleProcess(); 		                    //Schedule next process
		burst = running.CPUBursts[running.iCPU];	                    //set current burst
		TQ = running.Q * 7;				                    //set time quantum

		Console.WriteLine("Current CPU Time: {0}", currentT);
		Console.WriteLine("Now Running: {0}", running.ToString());
	
		PrintReadyQueue();
		PrintIOList();
	    }
	    public void EnterIO(Process p)                              //Send Process to IO    
            {
            	    ++p.iCPU;   					        //prepare next CPU burst iterator

	            if (p.iCPU <= p.CPUBursts.Length - 1)
        	    {
                	p.inIO = true; 					        //Enter IO state
	                p.arrivalT = currentT + p.IOTimes[p.iIO];               //prepare Arrival for after IO
        	        ToReadyQueue(p); 				        //back to ready queue after I/O time
	            }
        	    else
	            {
        	        p.finishT = currentT;               
                	Completed.Add(p);                                  	//Add to final list if no more bursts
	            }

            	    running = null;
        }
        public bool Preempt()                                           //MLFQ preemption       
        {
            if(running.Q != 1)
            {
                Process p = front;

                while (p != null && p.arrivalT <= currentT)
                {
                    if(!p.inIO && p.Q < running.Q)
                    {
                        return true;        				//preempt if not in io and high priority
                    }

                    p = p.next;
                }
            }

            return false;
        }
        public override Process ScheduleProcess()		         //Look for higher priorities and pop                        
        {
            Process p;

            if (currentT == 0)                                           //Schedule front of Ready Queue to start
            {
                p = front;
                front = front.next;
                p.next = null;
            }
            else                                                         //Find next process to schedule
            {
                p = FindNextProcess();
            }

            if (p.responseT == null)
            {
                p.responseT = currentT - p.arrivalT;
            }

            p.waitingT += currentT - p.arrivalT;

            return p;
        }
        public override void PrintReadyQueue()                           //Also display Priority Queue                               
        {
            Process p = front;

            Console.WriteLine("-------------------------------------");
            Console.WriteLine("Ready Queue:");
            Console.WriteLine("Process     Burst     Priority");

            while (p != null)
            {
                if (!p.inIO)
                {
                    Console.WriteLine(p.ToString().PadRight(12) + p.CPUBursts[p.iCPU].ToString().PadRight(10) + "Q" + p.Q.ToString());
                }

                p = p.next;
            }
        }
        private Process FindNextProcess()                               //MLFQ ScheduleProcess helper function                      
        {
            bool foundCandidate = false;
            Process p = front;
            Process b = front;

            while (p != null)
            {
                if(!p.inIO && p.arrivalT <= currentT && p.Q == 1)       //find next Q1
                {
                    foundCandidate = true;
                    break;
                }

                b = p;
                p = p.next;
            }

            if(!foundCandidate)
            {
                p = front;
                b = p;

                while (p != null)
                {
                    if (!p.inIO && p.arrivalT <= currentT && p.Q == 2)   //find next Q2
                    {
                        foundCandidate = true;
                        break;
                    }

                    b = p;
                    p = p.next;
                }
            }

            if(!foundCandidate)
            {
                p = front;
                b = p;

                while (p != null)
                {
                    if (!p.inIO && p.arrivalT <= currentT && p.Q == 3)    //find next Q1
                    {
                        foundCandidate = true;
                        break;
                    }

                    b = p;
                    p = p.next;
                }
            }

            b.next = p.next;
            if (p == front) { front = front.next; }
            p.next = null;

            return p;
        }
    }

    public class Scheduling                                                 //PARENT SCHEDULING CLASS
    { 
        //Field
	    public int burst = 0;
	    public int currentT = 0;
	    public int CPUIdleT = 0;		                            //time all processes in IO
	    public Process front;	
	    public Process running = null;
	    public List<Process> Completed = new List<Process>();           //place holder for finished processes

        //Methods
	    public virtual void ToReadyQueue( Process p )                   //Insert to ready queue linked-list                                 
	    {
		    if( front == null )
		    {
			    front = p;
		    }
		    else if( front.arrivalT > p.arrivalT )
		    {
		    	p.next = front;
		    	front = p;
		    }
		    else
		    {
		    	Process f = front;
		    	Process b = front;

		    	while( f != null && f.arrivalT <= p.arrivalT )
		    	{
		    		b = f;
		    		f = f.next;
		    	}

		    	b.next = p;
		    	p.next = f;
		    }
	    }
	    public virtual Process ScheduleProcess()		    //pop front                                                         
	    {
		    Process p = front;

		    front = front.next;
		    p.next = null;

		    if( p.responseT == null )
		    {
		    	p.responseT = currentT - p.arrivalT;
		    }

		    p.waitingT += currentT - p.arrivalT;

		    return p;
	    }
        public virtual void PrintReadyQueue()               //Show processes in ready queue                                     
        {
            Process p = front;

            Console.WriteLine("-------------------------------------");
            Console.WriteLine("Ready Queue:");
            Console.WriteLine("Process     Burst");

            while (p != null)
            {
                if (!p.inIO)
                {
                    Console.WriteLine(p.ToString().PadRight(12) + p.CPUBursts[p.iCPU]);
                }

                p = p.next;
            }
        }
        public virtual void PrintIOList()                   //Show processes in IO                                              
        {
            Process p = front;

            Console.WriteLine("-------------------------------------");
            Console.WriteLine("Now in I/O:");
            Console.WriteLine("Process     Remaining I/O time");

            while (p != null)
            {
                if (p.inIO)
                {
                    Console.WriteLine(p.ToString().PadRight(12) + p.IOTimes[p.iIO]);
                }

                p = p.next;
            }

            Console.WriteLine("-------------------------------------");
            Console.WriteLine("-------------------------------------");
            Console.WriteLine();
        }
        public virtual void UpdateIO()                      //Decrement all processes in IO to show remaining IO time           
        {
            Process p = front;

            while (p != null)
            {
                if (p.inIO)
                {
                    --p.IOTimes[p.iIO];		                     //decrement io time

                    if (p.IOTimes[p.iIO] == 0)
                    {
                        p.inIO = false;		                     //Back to Ready Queue
                        ++p.iIO;                                     //Prepare next IO iterator
                    }			
                }					

                p = p.next;
            }
        }
	    public virtual void DisplayResults(int type)  //Display averge times when all processes have completed            
	    {
            	double totalWait = 0;
	        double totalResponse = 0;
        	double turnAround = 0;
	    	double avgWait = 0;
	    	double avgResponse = 0;
	    	double avgTurn = 0;
	        double CPU = 0;

		Console.WriteLine("-------------------------------------");
                if (type == 0) { Console.WriteLine("FCFS Results:"); }
                else { Console.WriteLine("MLFQ Results:"); }
		Console.WriteLine("-------------------------------------");
		Console.WriteLine("Process      RT      WT      TT      ");

                Completed.Sort((a, b) => a.ToString().CompareTo(b.ToString()));     //Sort results list of completed processes
  
	    	foreach(Process p in Completed)
	    	{
	    		Console.WriteLine(p.ToString().PadRight(13) + p.responseT.ToString().PadRight(8) + p.waitingT.ToString().PadRight(8) + (p.finishT - p.responseT).ToString());

		    	totalWait += p.waitingT;
		    	totalResponse += p.responseT.Value;
                	turnAround += (p.finishT - p.responseT.Value);
		}

	    	Console.WriteLine("-------------------------------------");
		Console.WriteLine("Average      RT      WT      TT      ");

	       	avgWait = Math.Round(totalWait / 9, 2);
		avgResponse = Math.Round(totalResponse / 9, 2);
		avgTurn = Math.Round(turnAround / 9, 2);

                Console.WriteLine("".PadLeft(13) + avgResponse.ToString().PadRight(8) + avgWait.ToString().PadRight(8) + avgTurn.ToString());
                Console.WriteLine();

                CPU = (1.0000 - Math.Round((Convert.ToDouble(CPUIdleT) / Convert.ToDouble(currentT)), 4)) * 100;
                Console.WriteLine("Finish Time: {0}", currentT);
                Console.WriteLine("CPU UTILIZATION: {0}%", CPU);
	    }
    }

    class Program                                                       
    {
        static void Main(string[] args)
        {
            int[] Burst1 = { 12, 10, 15, 11, 9, 10, 11 };
            int[] Burst2 = { 18, 17, 16, 7, 17, 11, 12, 14 };
            int[] Burst3 = { 21, 15, 5, 9, 11, 14, 8 };
            int[] Burst4 = { 5, 4, 6, 8, 4, 3, 6, 5 };
            int[] Burst5 = { 4, 6, 5, 4, 5, 14, 7, 12, 15 };
            int[] Burst6 = { 7, 5, 6, 5, 4, 7, 6, 5 };
            int[] Burst7 = { 22, 7, 5, 24, 4, 3, 6, 6, 5 };
            int[] Burst8 = { 25, 20, 16, 7, 14, 15, 4, 3, 5, 4 };
            int[] Burst9 = { 3, 14, 8, 4, 7, 5, 4, 5, 16 };

            int[] IO1 = { 44, 52, 21, 42, 31, 77 };
            int[] IO2 = { 31, 42, 27, 41, 33, 43, 32 };
            int[] IO3 = { 24, 27, 28, 26, 49, 55 };
            int[] IO4 = { 35, 41, 45, 51, 61, 54, 61 };
            int[] IO5 = { 41, 26, 38, 33, 37, 28, 18, 33 };
            int[] IO6 = { 33, 31, 32, 41, 42, 39, 33 };
            int[] IO7 = { 38, 41, 29, 26, 32, 22, 26, 36 };
            int[] IO8 = { 21, 33, 41, 21, 23, 31, 32, 32, 41 };
            int[] IO9 = { 37, 41, 30, 19, 33, 18, 26, 31 };

            FCFSSimulation FCFS = new FCFSSimulation();
            MLFQSimulation MLFQ = new MLFQSimulation();

            FCFS.ToReadyQueue(new Process((int[])Burst1.Clone(), (int[])IO1.Clone(), 1));
            FCFS.ToReadyQueue(new Process((int[])Burst2.Clone(), (int[])IO2.Clone(), 2));
            FCFS.ToReadyQueue(new Process((int[])Burst3.Clone(), (int[])IO3.Clone(), 3));
            FCFS.ToReadyQueue(new Process((int[])Burst4.Clone(), (int[])IO4.Clone(), 4));
            FCFS.ToReadyQueue(new Process((int[])Burst5.Clone(), (int[])IO5.Clone(), 5));
            FCFS.ToReadyQueue(new Process((int[])Burst6.Clone(), (int[])IO6.Clone(), 6));
            FCFS.ToReadyQueue(new Process((int[])Burst7.Clone(), (int[])IO7.Clone(), 7));
            FCFS.ToReadyQueue(new Process((int[])Burst8.Clone(), (int[])IO8.Clone(), 8));
            FCFS.ToReadyQueue(new Process((int[])Burst9.Clone(), (int[])IO9.Clone(), 9));

            MLFQ.ToReadyQueue(new Process((int[])Burst1.Clone(), (int[])IO1.Clone(), 1));
            MLFQ.ToReadyQueue(new Process((int[])Burst2.Clone(), (int[])IO2.Clone(), 2));
            MLFQ.ToReadyQueue(new Process((int[])Burst3.Clone(), (int[])IO3.Clone(), 3));
            MLFQ.ToReadyQueue(new Process((int[])Burst4.Clone(), (int[])IO4.Clone(), 4));
            MLFQ.ToReadyQueue(new Process((int[])Burst5.Clone(), (int[])IO5.Clone(), 5));
            MLFQ.ToReadyQueue(new Process((int[])Burst6.Clone(), (int[])IO6.Clone(), 6));
            MLFQ.ToReadyQueue(new Process((int[])Burst7.Clone(), (int[])IO7.Clone(), 7));
            MLFQ.ToReadyQueue(new Process((int[])Burst8.Clone(), (int[])IO8.Clone(), 8));
            MLFQ.ToReadyQueue(new Process((int[])Burst9.Clone(), (int[])IO9.Clone(), 9));

            Console.BufferHeight = 3500;                            //To ensure entire output is displayed

            Console.WriteLine("/////////////////////////////////////////////////");
            Console.WriteLine("FCFS SIMULATION");
            Console.WriteLine("/////////////////////////////////////////////////");

            FCFS.StartScheduling();
            FCFS.DisplayResults(0);

            Console.WriteLine();
            Console.WriteLine("/////////////////////////////////////////////////");
            Console.WriteLine("MLFQ SIMULATION");
            Console.WriteLine("/////////////////////////////////////////////////");

            MLFQ.StartScheduling();
            MLFQ.DisplayResults(1);

            Console.ReadLine();                                     //To keep console open after execution
        }
    }
}
