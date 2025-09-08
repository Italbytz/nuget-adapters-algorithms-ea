import math

with open("problem.dat", "w") as f:
    f.write("1 100 -5 5 63\n")
    x = 0.0
    while x <= 6.2:
        f.write(f"{x:.1f} {math.sin(x):.6f}\n")
        x += 0.1