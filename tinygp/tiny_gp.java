/* 
 * Program:      tiny_gp.java
 *
 * Author:       Riccardo Poli (email: rpoli@essex.ac.uk)
 *
 * $Log: tiny_gp.java,v $
   WBL Sun Feb 20 2005 Use GPQUICK format. Increase GENERATIONS = 500
   Add Revision: 1.00, fname, goodfit (based on backward_chain_gp.java r1.8)
   Add asserts (requires javac -source 1.4)

   WBL Sun Feb 20 11:54:26 GMT 2005 BUGFIX ensure can set numberconst to zero
   WBL Tue Dec 14 15:30:44 GMT 2004
 * Rename randomnumber to numberconst, add create_random_leaf() and make
 *   mutation and initial population use it (nb chance of any constant 50%
 *   and any input 50%, not dominated by number of constants), check varnumber+numberconst
 *   does not violoate FSET_START.

   WBL 12 Dec 2004 Set up x[] before create_random_pop

 * Revision 1.5  2004/11/04 09:52:38  rpoli
 * Version with constants and primitive documentation. Not backward
 * compatible with previous format for input data file.
 *
 *
 */

import java.util.Random;
import java.io.BufferedReader;
import java.io.FileReader;
import java.util.StringTokenizer;
import java.io.IOException;

/**
 * Describe class <code>tiny_gp</code> here.
 *
 * @author <a href="mailto:rpoli@essex.ac.uk">Riccardo Poli</a>
 * @version 1.0
 */

public class tiny_gp {
    double [] fitness;
    char [][] pop;
    static Random rd = new Random();
    static final int 
	ADD = 110, 
	SUB = 111, 
	MUL = 112, 
	DIV = 113,
	FSET_START = ADD, 
	FSET_END = DIV;
    static double [] x = new double[FSET_START];
    static double minrandom, maxrandom;
    static char [] program;
    static int PC;
    static int varnumber, fitnesscases, numberconst;
    static double fbestpop = 0.0, favgpop = 0.0;
    static long seed;
    static double avg_len; 
    static long nodes_evaluated = 0;
    static final int  
	MAX_LEN = 10000,  
	POPSIZE = 10000,
	DEPTH   = 5,
	GENERATIONS = 50,
	TSIZE = 2;
    /**
     * Describe constant <code>PMUT_PER_NODE</code> here.
     *
     */
    public static final double  
	PMUT_PER_NODE  = 0.02,
	/**
	 * Describe constant <code>CROSSOVER_PROB</code> here.
	 *
	 */
	CROSSOVER_PROB = 0.9,
	goodfit = -1e-5;
    static double [][] targets;


    double run() /* Interpreter */
    {
	char primitive = program[PC++];
	if ( primitive < FSET_START )
	    return(x[primitive]);
	switch ( primitive )	
	    {
	    case ADD : return( run() + run() );
	    case SUB : return( run() - run() );
	    case MUL : return( run() * run() );
	    case DIV : 
		{ double num = run(), den = run();
		if ( Math.abs( den ) <= 0.001 ) 
		    return( num );
		else 
		    return( num / den );
		}
	    }

        assert false; 
	return( 0.0 ); // should never get here
    }
	    
    class Pair { int max=0, current=0; }
    int depth( char [] buffer, int buffercount, Pair depth )
    {
	final int current = depth.current+1;
	if(current>depth.max) depth.max=current;

	if ( buffer[buffercount] < FSET_START )
	    return( ++buffercount );
	/*Else two arguments*/
	assert
	    buffer[buffercount]== ADD ||
	    buffer[buffercount]== SUB ||
	    buffer[buffercount]== MUL ||
	    buffer[buffercount]== DIV : buffer[buffercount];

	depth.current=current;
	final int a1 = depth( buffer, ++buffercount, depth );
	depth.current=current;
	return depth( buffer, a1, depth );
    }
    int traverse( char [] buffer, int buffercount )
    {
	if(buffercount==0)
	    return buffer.length;
	
	if ( buffer[buffercount] < FSET_START )
	    return( ++buffercount );
	
	switch(buffer[buffercount])
	    {
	    case ADD: 
	    case SUB: 
	    case MUL: 
	    case DIV: 
		return( traverse( buffer, traverse( buffer, ++buffercount ) ) );
	    }
        assert false; 
	return( 0 ); // should never get here
    }



    void setup_fitness(String fname)
    {
	/*
        DataInputStream in;
	int i,j;
	try {
	    in = 
		new DataInputStream(
				    new
				    FileInputStream(new File(fname)));
	*/
	try {
	    int i,j;
	    String line;
	    
	    BufferedReader in = 
		new BufferedReader(
				    new
				    FileReader(fname));
	    line = in.readLine();
	    // System.out.println(line); 
	    StringTokenizer tokens = new StringTokenizer(line);
	    varnumber = Integer.parseInt(tokens.nextToken().trim());
	    numberconst = Integer.parseInt(tokens.nextToken().trim());
	    minrandom =	Double.parseDouble(tokens.nextToken().trim());
	    maxrandom =  Double.parseDouble(tokens.nextToken().trim());
	    fitnesscases = Integer.parseInt(tokens.nextToken().trim());
	    // System.out.println(varnumber + " " + fitnesscases );
	    targets = new double[fitnesscases][varnumber+1];
	    if (varnumber+numberconst>= FSET_START ) System.out.println("too many variables");
	    if (fitnesscases >= 1000 ) System.out.println("too many fitness cases");
	    
	    for (i = 0; i < fitnesscases; i ++ )
		{
		    line = in.readLine();
		    tokens = new StringTokenizer(line);
		    for (j = 0; j <= varnumber; j++)
			{
			targets[i][j] = Double.parseDouble(tokens.nextToken().trim());
			// System.out.println(targets[i][j]);
			}
		}
	    in.close();
	}
	catch(IOException e)
	    {
		System.out.println("Some IOException occured");
	    }


    }



    double fitness_function( char [] Prog ) 
    {
	int i = 0, len;
	double result, fit = 0.0;
	
	len = traverse( Prog, 0 );
  
	for (i = 0; i < fitnesscases; i ++ )
	    {
		for (int j = 0; j < varnumber; j ++ )
		    x[j] = targets[i][j];
		program = Prog;
		PC = 0;
		result = run();
		nodes_evaluated += len;
		
		fit += Math.abs( result - targets[i][varnumber]);
	    }
	
	return(-fit );
    }


    char create_random_leaf()
    {
	if(numberconst==0 || rd.nextInt(2)==0) 
	    return (char) rd.nextInt(varnumber);
	else
	    return (char) (varnumber + rd.nextInt(numberconst));
    }

    int grow( char [] buffer, int pos, int max, int depth )
    {
	char prim = (char) rd.nextInt(2);

	if ( pos >= max ) 
	    return( -1 );
	
	if ( pos == 0 )
	    prim = 1;
	
	if ( prim == 0 || depth == 0 )
	    {
		buffer[pos] = create_random_leaf();
		return(pos+1);
	    }
	else
	    {
		prim = (char) (rd.nextInt(FSET_END - FSET_START + 1) + FSET_START);
		switch(prim)
		    {
		    case ADD: 
		    case SUB: 
		    case MUL: 
		    case DIV:
			
			buffer[pos] = prim;
			return( grow( buffer, grow( buffer, pos+1, max,depth-1), max,depth-1 ) );
		    }
	    }
	return( 0 ); // should never get here
    }
    
    int print_indiv( char []buffer, int buffercounter )
    {
	int a1=0, a2;
	if ( buffer[buffercounter] < FSET_START )
	    {
		if ( buffer[buffercounter] < varnumber )
		    System.out.print( "X"+ (buffer[buffercounter] + 1 )+ " ");
		else
		    System.out.print( x[buffer[buffercounter]]);
		return( ++buffercounter );
	    }
	switch(buffer[buffercounter])
	    {
		/*
	    case ADD: System.out.print( "( ");
			  a1=print_indiv( buffer, ++buffercounter ); 
			  System.out.print( " + "); 
			  break;
	    case SUB: System.out.print( "( ");
			  a1=print_indiv( buffer, ++buffercounter ); 
			  System.out.print( " - "); 
			  break;
	    case MUL: System.out.print( "( ");
			  a1=print_indiv( buffer, ++buffercounter ); 
			  System.out.print( " * "); 
			  break;
	    case DIV: System.out.print( "pdiv( ");
			  a1=print_indiv( buffer, ++buffercounter ); 
			  System.out.print( ", "); 
			  break;
		Replaced by GPQUICK S-EXPRESSION FORMAT*/
	    case ADD: System.out.print( "(ADD ");
			  break;
	    case SUB: System.out.print( "(SUB ");
			  break;
	    case MUL: System.out.print( "(MUL ");
			  break;
	    case DIV: System.out.print( "(DIV ");
			  break;
	    }
	a1=print_indiv( buffer, ++buffercounter ); 
	a2=print_indiv( buffer, a1 ); 
	System.out.print( ")");
	return( a2);
    }
    

    static char [] buffer = new char[MAX_LEN];
    char [] create_random_indiv( int depth )
    {
	char [] ind;
	int len;

	len = grow( buffer, 0, MAX_LEN, depth );
  
	while (len < 0 )
	    len = grow( buffer, 0, MAX_LEN, depth );

	ind = new char[len];

	System.arraycopy(buffer, 0, ind, 0, len ); 
	return( ind );
    }

    char [][] create_random_pop(int n, int depth, double [] fitness )
    {
	char [][]pop = new char[n][];
	int i;
	
	for ( i = 0; i < n; i ++ )
	    {
		pop[i] = create_random_indiv( depth );
		fitness[i] = fitness_function( pop[i] );
	    }
	return( pop );
    }


    void stats( double [] fitness, char [][] pop, int gen )
    {
	int i, best = rd.nextInt(POPSIZE);
	int node_count = 0;
	fbestpop = fitness[best];
	favgpop = 0.0;

	for ( i = 0; i < POPSIZE; i ++ )
	    {
		node_count +=  traverse( pop[i], 0 );
		favgpop += fitness[i];
		if ( fitness[i] > fbestpop )     
		    {
			best = i;
			fbestpop = fitness[i];
		    }
	    }
	avg_len = (double) node_count / POPSIZE;
	favgpop /= POPSIZE;
	System.out.print("Generation="+gen+" Avg Fitness="+(-favgpop)+" Best=pop["+best+"] Fitness="+(-fbestpop)+"\nNodes Eval="+nodes_evaluated+"  Avg_size="+avg_len+"\n");
	Pair D = new Pair();
	final int size = depth(pop[best],0,D );
	assert size==pop[best].length : size;
	assert D.max<=(1+pop[best].length/2) : D.max;
	System.out.println("size "+size+" depth "+D.max);
	print_indiv( pop[best], 0 );
	System.out.print( "\n");
	System.out.flush();
    }

    int tournament( double [] fitness, int tsize )
    {
	int best = rd.nextInt(POPSIZE), i, competitor;
	double  fbest = -1.0e34;
	
	for ( i = 0; i < tsize; i ++ )
	    {
		competitor = rd.nextInt(POPSIZE);
		if ( fitness[competitor] > fbest )
		    {
			fbest = fitness[competitor];
			best = competitor;
		    }
	    }
	return( best );
    }
    
    int negative_tournament( double [] fitness, int tsize )
    {
	int worst = rd.nextInt(POPSIZE), i, competitor;
	double fworst = 1e34;
	
	for ( i = 0; i < tsize; i ++ )
	    {
		competitor = rd.nextInt(POPSIZE);
		if ( fitness[competitor] < fworst )
		    {
			fworst = fitness[competitor];
			worst = competitor;
		    }
	    }
	return( worst );
    }
    
    char [] crossover( char []parent1, char [] parent2 )
    {
	int xo1start, xo1end, xo2start, xo2end;
	char [] offspring;
	int len1 = traverse( parent1, 0 );
	int len2 = traverse( parent2, 0 );
	int lenoff;
	
	xo1start =  rd.nextInt(len1);
	xo1end = traverse( parent1, xo1start );
	
	xo2start =  rd.nextInt(len2);
	xo2end = traverse( parent2, xo2start );
	
	lenoff = xo1start + (xo2end - xo2start) + (len1-xo1end);

	offspring = new char[lenoff];

	System.arraycopy( parent1, 0, offspring, 0, xo1start );
	System.arraycopy( parent2, xo2start, offspring, xo1start,  
			  (xo2end - xo2start) );
	System.arraycopy( parent1, xo1end, offspring, 
			  xo1start + (xo2end - xo2start), 
			  (len1-xo1end) );
  
	return( offspring );
    }
    
    char [] mutation( char [] parent, double pmut )
    {
	int len = traverse( parent, 0 ), i;
	int mutsite;
	char [] parentcopy = new char [len];

	System.arraycopy( parent, 0, parentcopy, 0, len );
	for (i = 0; i < len; i ++ )
	    {  
		if ( rd.nextDouble() < pmut )
		    {
			mutsite =  i;
			if ( parentcopy[mutsite] < FSET_START )
			    parentcopy[mutsite] = create_random_leaf();
			else
			    switch(parentcopy[mutsite])
				{
				case ADD: 
				case SUB: 
				case MUL: 
				case DIV:
				    parentcopy[mutsite] = (char) (rd.nextInt(FSET_END - FSET_START + 1) + FSET_START);
				}
		    }
	    }
	return( parentcopy );
    }


    void print_parms()
    {
	System.out.print("\n\n--$Revision: 1.00 $------------------\nMultivariate polynomial problem\n----------------------------------\n");
	System.out.print("SEED="+seed+"\nMAX_LEN="+MAX_LEN+
			      "\nPOPSIZE="+POPSIZE+"\nDEPTH="+DEPTH+
			      "\nCROSSOVER_PROB="+CROSSOVER_PROB+
			      "\nPMUT_PER_NODE="+PMUT_PER_NODE+
			      "\nMIN_RANDOM="+minrandom+
			      "\nMAX_RANDOM="+maxrandom+
			      "\nGENERATIONS="+GENERATIONS+
			      "\ngoodfit="+goodfit+
			      "\nTSIZE="+TSIZE+
			      "\n----------------------------------\n");
    }

    /**
     * Creates a new <code>tiny_gp</code> instance.
     *
     * @param fname a <code>String</code> value
     * @param s a <code>long</code> value
     */
    public tiny_gp( String fname, long s )
	{
	    fitness =  new double[POPSIZE];
	    seed = s;
	    if ( seed >= 0 )
		rd.setSeed(seed);
	    setup_fitness(fname);
	    for ( int i = 0; i < FSET_START; i ++ )
		x[i]= (maxrandom-minrandom)*rd.nextDouble()+minrandom;
	    pop = create_random_pop(POPSIZE, DEPTH, fitness );
	}

    void evolve()
    {
	int gen = 0, indivs, offspring, parent1, parent2,   parent;
	double newfit;
	char []newind;
	print_parms();
	stats( fitness, pop, 0 );
	for ( gen = 1; gen < GENERATIONS; gen ++ )
	    {
		if (  fbestpop > goodfit ) 
		    {
			System.out.print("PROBLEM SOLVED\n");
			System.exit( 0 );
		    }
		for ( indivs = 0; indivs < POPSIZE; indivs ++ )
		    {
			if ( rd.nextDouble() > CROSSOVER_PROB  )
			    {
				parent1 = tournament( fitness, TSIZE );
				parent2 = tournament( fitness, TSIZE );
				newind = crossover( pop[parent1],pop[parent2] );
			    }
			else
			    {
				parent = tournament( fitness, TSIZE );
				newind = mutation( pop[parent], PMUT_PER_NODE );
			    }
			newfit = fitness_function( newind );
			offspring = negative_tournament( fitness, TSIZE );
			//			free( pop[offspring] );
			pop[offspring] = newind;
			fitness[offspring] = newfit;
		    }
		stats( fitness, pop, gen );
	    }
	System.out.print("PROBLEM *NOT* SOLVED\n");
	System.exit( 1 );
    }

    /**
     * Describe <code>main</code> method here.
     *
     * @param args a <code>String[]</code> value
     */
    public static void main(String[] args) 
    {
	String fname;
	long s;
	
	// for ( int l = 0; l < args.length; l++)
	// System.out.println(args[l]);

	if ( args.length >= 1 ) 
	    {
		s = Integer.valueOf(args[0]).intValue();
	    }
	else
	    s = -1;

	fname = args.length >= 2 ? args[1] : "problem.dat";
	// System.out.println(s );
	System.out.println("problem "+fname );
	tiny_gp gp = new tiny_gp(fname, s);
	gp.evolve();
    }

};
 