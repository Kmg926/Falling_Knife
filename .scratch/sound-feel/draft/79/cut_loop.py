import numpy as np,sys,wave
sr=48000
x=np.fromfile(sys.argv[1],dtype=np.float32).reshape(-1,2)
a,b=np.load(sys.argv[2])
pk=float(np.abs(x).max()); g=0.98/pk; print("peak %.3f gain %.3f (%.2f dB)"%(pk,g,20*np.log10(g))); x=x*g
def w(path,d):
    d=np.clip(d,-1,1); i=(d*32767).round().astype('<i2')
    with wave.open(path,'wb') as f:
        f.setnchannels(2); f.setsampwidth(2); f.setframerate(sr); f.writeframes(i.tobytes())
    print(path.split('/')[-1], len(d), 'samples %.5fs'%(len(d)/sr), 'clipped', int((np.abs(d)>1).sum()))
out=sys.argv[3]
w(out+'/xjj9-intro.wav',x[:a]); w(out+'/xjj9-loop.wav',x[a:b])
