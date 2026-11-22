import numpy as np,sys
sr=48000
x=np.fromfile(sys.argv[1],dtype=np.float32).reshape(-1,2)
m=x.mean(1)
b0=int(151.527*sr); a0=int(62.910*sr)
W=int(1.0*sr)
# 루프 길이(b-a)를 고정 후보 주변에서 1샘플 단위로: 점프 앞 1초(b 앞)와 a 앞 1초가 같고, 뒤도 같도록
best=None
for d in range(-int(0.01*sr),int(0.01*sr)):
    L=b0-a0+d
    a=a0; b=a+L
    s1=m[b-W:b+W]; s2=m[a-W:a+W]
    c=np.dot(s1,s2)/(np.linalg.norm(s1)*np.linalg.norm(s2))
    if best is None or c>best[0]: best=(c,L)
c,L=best; print('best len samples',L,'sec %.5f corr %.5f'%(L/sr,c))
# 점프 시작점 a를 0.5초 범위에서 옮겨, 점프 순간 파형 차이가 가장 작은 곳(클릭 없음)
bestj=None
for a in range(a0-int(0.25*sr),a0+int(0.25*sr)):
    b=a+L
    # 점프: b-1 다음에 a가 나옴. 경계 앞뒤 32샘플 차이
    e=np.mean((x[b-32:b+32]-x[a-32:a+32])**2)
    if bestj is None or e<bestj[0]: bestj=(e,a)
e,a=bestj; b=a+L
print('a=%d (%.5fs) b=%d (%.5fs) mse=%.2e  jump step L=%.5f R=%.5f, local amp %.3f'%(a,a/sr,b,b/sr,e,x[b,0]-x[a,0],x[b,1]-x[a,1],np.abs(x[a-200:a+200]).max()))
np.save(sys.argv[2],np.array([a,b]))
