// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------
  
using UnityEngine;


namespace ControlFreak2.Internal
{
	
// -----------------------
//! Joystick state class.
// -----------------------
public class JoystickState 
	{
//! \cond
	public JoystickConfig
		config;
		

	private float
		tilt,
		angle,
		safeAngle;

	private Vector2
		pos,
		posRaw,
		normDir;
		//normDirRaw;
	

	private DirectionState
		dirState,
		dirState4,
		dirState8;

	private Dir 
		dirLastNonNeutral4,
		dirLastNonNeutral8;

	

	private Vector2 
		nextFramePos; // for accumulative setting
	private bool
		nextFrameDigiU,
		nextFrameDigiR,
		nextFrameDigiD,
		nextFrameDigiL;		
		
	
	private Vector2
		digiInputAnalogVecCur;


	const float MIN_SAFE_TILT			= 0.01f;
	const float MIN_SAFE_TILT_SQ		= MIN_SAFE_TILT * MIN_SAFE_TILT;



	// ---------------------
	public JoystickState(JoystickConfig config)
		{
		this.config = config;

		this.dirState = new DirectionState();
		this.dirState4 = new DirectionState();
		this.dirState8 = new DirectionState();

			
		this.Reset();
		}

//! \endcond

	// --------------------------
	//! Get current (transformed) vector.
	// --------------------------
	public Vector2	GetVector()	 				{ return this.pos; }

	// --------------------------
	//! Get current raw vector.
	// --------------------------
	public Vector2	GetVectorRaw() 			{ return this.posRaw; }

	// -----------------------
	//! Get curent transformed vector in given clamp mode.
	// -----------------------
	public Vector2 GetVectorEx(bool squareMode)
		{
		if ((this.config.clampMode == JoystickConfig.ClampMode.Square) == squareMode)
			return this.GetVector();
		else
			return (squareMode ? CFUtils.CircularToSquareJoystickVec(this.GetVector()) : CFUtils.SquareToCircularJoystickVec(this.GetVector()));
		} 

	// -----------------------
	//! Get curent raw vector in given clamp mode.
	// -----------------------
	public Vector2 GetVectorRawEx(bool squareMode)
		{
		if ((this.config.clampMode == JoystickConfig.ClampMode.Square) == squareMode)
			return this.GetVectorRaw();
		else
			return (squareMode ? CFUtils.CircularToSquareJoystickVec(this.GetVectorRaw()) : CFUtils.SquareToCircularJoystickVec(this.GetVectorRaw()));
		} 



	// --------------------------
	//! Get current angle in degrees.
	// --------------------------
	public float 	GetAngle()					{ return this.angle; }

	// --------------------------
	//! Get current tilt (0 - 1).
	// --------------------------
	public float 	GetTilt()					{ return this.tilt; }


	// --------------------------
	public DirectionState	GetDirState()		{ return this.dirState; }					//!< Get digital direction state (4- or 8-way, according to joystick config).
	public DirectionState	GetDirState4()		{ return this.dirState4; }					//!< Get digital 4-way direction state.
	public DirectionState	GetDirState8()		{ return this.dirState8; }					//!< Get digital 8-way direction state.
	public Dir		GetDir()							{ return this.dirState.GetCur(); }		//!< Get current digital direction (according to config).
	public Dir		GetDir4()						{ return this.dirState4.GetCur(); }		//!< Get current digital 4-way direction.
	public Dir		GetDir8()						{ return this.dirState8.GetCur(); }		//!< Get current digital 8-way direction.
	public bool 	JustPressedDir	(Dir dir)	{ return this.dirState.JustPressed(dir); }	//!< Just changed direction to given one?
	public bool 	JustPressedDir4(Dir dir)	{ return this.dirState4.JustPressed(dir); }	//!< Just changed direction to given one? (4-way)
	public bool 	JustPressedDir8(Dir dir)	{ return this.dirState8.JustPressed(dir); }	//!< Just changed direction to given one? (8-way)
	public bool 	JustReleasedDir(Dir dir)	{ return this.dirState.JustReleased(dir); }	//!< Just changed direction from given one?
	public bool 	JustReleasedDir4(Dir dir)	{ return this.dirState4.JustReleased(dir); }	//!< Just changed direction from given one? (4-way)
	public bool 	JustReleasedDir8(Dir dir)	{ return this.dirState8.JustReleased(dir); }	//!< Just changed direction from given one? (8-way)



	// -----------------------
	//! Set this state's joystick config.
	// ---------------------
	public void	SetConfig(JoystickConfig config)	
		{ this.config = config;	}


	// -----------------
	//! Reset joystick state.	
	// ---------------
	public void Reset()
		{
		this.normDir = Vector2.up;
		this.safeAngle = 0;


		this.dirState.Reset();
		this.dirState4.Reset();
		this.dirState8.Reset();

//		this.dirCur = this.dirPrev =
//		this.dirCur4 = this.dirPrev4 =
//		this.dirCur8 = this.dirPrev8 = Dir.N;	
//		this.dirFirst = this.dirFirst4 = this.dirFirst8 = Dir.N;

		this.nextFrameDigiD = this.nextFrameDigiL =
		this.nextFrameDigiR = this.nextFrameDigiU = false;

		this.nextFramePos = Vector2.zero;
		this.digiInputAnalogVecCur = Vector2.zero;
		//this.digiInputDirPrev = Dir.N;
		
		this.angle = 0;
		this.dirLastNonNeutral4 = Dir.N;
		this.dirLastNonNeutral8 = Dir.N;
		this.normDir = new Vector2(0, 1);
		//this.normDirRaw = new Vector2(0, 1);

		this.pos = this.posRaw = Vector2.zero;
		}
	

//! \cond

	// ----------------
	public void ApplyUnclampedVec(Vector2 v) //, JoystickConfig.ClampMode sourceClampMode)
		{

		if (this.config.clampMode == JoystickConfig.ClampMode.Circle)	
			v = CFUtils.ClampInsideUnitCircle(v);
		else
			v = CFUtils.ClampInsideUnitSquare(v);
			
		this.nextFramePos.x = CFUtils.ApplyDeltaInput(this.nextFramePos.x, v.x);
		this.nextFramePos.y = CFUtils.ApplyDeltaInput(this.nextFramePos.y, v.y);
		}


	// -------------------
	public void ApplyClampedVec(Vector2 v, JoystickConfig.ClampMode clampMode)
		{
		if (clampMode != this.config.clampMode)
			{
			if (this.config.clampMode == JoystickConfig.ClampMode.Circle)
				v = CFUtils.SquareToCircularJoystickVec(v);
			else
				v = CFUtils.CircularToSquareJoystickVec(v);
			}		

		this.nextFramePos.x = CFUtils.ApplyDeltaInput(this.nextFramePos.x, v.x);
		this.nextFramePos.y = CFUtils.ApplyDeltaInput(this.nextFramePos.y, v.y);
		}

		
	// -----------------	
	public void ApplyDir(Dir dir)
		{		
		this.ApplyDigital(
			((dir == Dir.U) || (dir == Dir.UL) || (dir == Dir.UR)),
			((dir == Dir.R) || (dir == Dir.UR) || (dir == Dir.DR)),
			((dir == Dir.D) || (dir == Dir.DL) || (dir == Dir.DR)),
			((dir == Dir.L) || (dir == Dir.UL) || (dir == Dir.DL)) );
		}
		

	// ----------------
	public void ApplyDigital(
		bool digiU,
		bool digiR,
		bool digiD,
		bool digiL)
		{
		if (digiU)	this.nextFrameDigiU = true;
		if (digiR)	this.nextFrameDigiR = true;
		if (digiD)	this.nextFrameDigiD = true;
		if (digiL)	this.nextFrameDigiL = true;
		}
		

	// ------------------
	public void ApplyState(JoystickState state)
		{
		if (this.config.stickMode == JoystickConfig.StickMode.Analog)
			this.ApplyClampedVec(state.GetVectorRaw(), state.config.clampMode);

		else if (this.config.stickMode == JoystickConfig.StickMode.Digital4)
			this.ApplyDir(state.GetDir4());
		else 
			this.ApplyDir(state.GetDir8());
			
		//this.ApplyDir(state.GetDir());
		}


	
	
	

	// ------------------
	public void Update()
		{
		// Combine digital and analog input...
			
		Dir digiInputDir = CFUtils.DigitalToDir(this.nextFrameDigiU, this.nextFrameDigiR, this.nextFrameDigiD, this.nextFrameDigiL);
			
		this.digiInputAnalogVecCur = this.config.AnimateDirToAnalogVec(this.digiInputAnalogVecCur, digiInputDir);

		
		this.posRaw.x = CFUtils.ApplyDeltaInput(this.nextFramePos.x, this.digiInputAnalogVecCur.x);
		this.posRaw.y = CFUtils.ApplyDeltaInput(this.nextFramePos.y, this.digiInputAnalogVecCur.y);


		this.nextFramePos	= Vector2.zero;
		this.nextFrameDigiU	= false;
		this.nextFrameDigiR = false;
		this.nextFrameDigiD = false;
		this.nextFrameDigiL = false;


			
			
		// Process input...

		if (this.config.blockX) this.posRaw.x = 0;
		if (this.config.blockY) this.posRaw.y = 0;
			

		Vector2 unclampedPos = this.posRaw;

		this.posRaw = this.config.ClampNormalizedPos(this.posRaw);
			
		float	tiltRaw		= this.posRaw.magnitude;
		float 	angleRaw 	= this.safeAngle;

		if (this.posRaw.sqrMagnitude < MIN_SAFE_TILT_SQ)
			{
			tiltRaw = 0;
			this.tilt = 0;
			} 	
		else
			{
			this.normDir = this.posRaw.normalized;
			this.tilt	= tiltRaw;
			angleRaw	= Mathf.Atan2(this.normDir.x, this.normDir.y) * Mathf.Rad2Deg;
			angleRaw 	= CFUtils.NormalizeAnglePositive(angleRaw);
			}
 
		// Get digital state...


			Dir 
				curDir4 = this.GetDir4(),
				curDir8 = this.GetDir8();


			float 
				dist4 = tiltRaw,
				dist8 = tiltRaw;


			// Use different tilt calculation mode...

			if (this.config.digitalDetectionMode == JoystickConfig.DigitalDetectionMode.Touch)
				{
				dist4 = Mathf.Max(Mathf.Abs(unclampedPos.x), Mathf.Abs(unclampedPos.y));

				dist8 = dist4;
				dist8 = Mathf.Max(dist8, Mathf.Abs(((CFUtils.OneOverSqrtOf2 * unclampedPos.x) + (CFUtils.OneOverSqrtOf2 * unclampedPos.y))));
				dist8 = Mathf.Max(dist8, Mathf.Abs(((CFUtils.OneOverSqrtOf2 * unclampedPos.x) - (CFUtils.OneOverSqrtOf2 * unclampedPos.y))));
				}
		


	
			if (tiltRaw < 0.001f)
				{
				this.dirLastNonNeutral4 = this.dirLastNonNeutral8 = Dir.N;
				}

			float enterThresh4 = ((this.dirLastNonNeutral4 != Dir.N) ? this.config.digitalEnterThresh : 
				(0.5f * (this.config.digitalEnterThresh + this.config.digitalLeaveThresh)));

			curDir4 = 
				(dist4 > enterThresh4) ? CFUtils.DirFromAngleEx(angleRaw, false, this.dirLastNonNeutral4, this.config.angularMagnet) : 
				(dist4 > this.config.digitalLeaveThresh) ? curDir4 : Dir.N;  


			float enterThresh8 = ((this.dirLastNonNeutral8 != Dir.N) ? this.config.digitalEnterThresh : 
				(0.5f * (this.config.digitalEnterThresh + this.config.digitalLeaveThresh)));

			curDir8 = 
				(dist8 > enterThresh8) ? CFUtils.DirFromAngleEx(angleRaw, true, this.dirLastNonNeutral8, this.config.angularMagnet) : 
				(dist8 > this.config.digitalLeaveThresh) ? curDir8 : Dir.N;  



		if (curDir4 != Dir.N)
			this.dirLastNonNeutral4 = curDir4;
		if (curDir8 != Dir.N)
			this.dirLastNonNeutral8 = curDir8;




		this.dirState.BeginFrame();
		this.dirState4.BeginFrame();
		this.dirState8.BeginFrame();


		this.dirState4.SetDir(curDir4, this.config.originalDirResetMode);
		this.dirState8.SetDir(curDir8, this.config.originalDirResetMode);
		this.dirState.SetDir((this.config.stickMode == JoystickConfig.StickMode.Digital4) ? this.GetDir4() : this.GetDir8(), 
			this.config.originalDirResetMode);


		switch (this.config.stickMode)
			{
			case JoystickConfig.StickMode.Analog :

				this.angle	= angleRaw;

				if (this.config.perAxisDeadzones)
					{
					this.pos.x = this.config.GetAnalogVal(this.posRaw.x);
					this.pos.y = this.config.GetAnalogVal(this.posRaw.y);
					this.tilt = this.pos.magnitude;
					}
				else
					{
					this.tilt = this.config.GetAnalogVal(tiltRaw);
					this.pos = this.normDir * this.tilt;	

					if (this.config.clampMode == JoystickConfig.ClampMode.Square)
						this.pos = CFUtils.CircularToSquareJoystickVec(this.pos);
					}
			
				break;

			case JoystickConfig.StickMode.Digital4 :
			case JoystickConfig.StickMode.Digital8 :
				
				this.pos	= CFUtils.DirToVector(this.GetDir(), (this.config.clampMode == JoystickConfig.ClampMode.Circle));
				this.angle	= CFUtils.DirToAngle(this.GetDir());
				this.tilt	= (this.GetDir() == Dir.N) ? 0 : 1;

				if (this.GetDir() != Dir.N)
					{
					this.normDir = this.pos.normalized;
					}

				break;
			}

				

		this.safeAngle = this.angle;

		}

		//! \endcond


		}


}

