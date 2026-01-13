# Simply Supported Steel Beam Design

This calculation checks a simply supported steel beam under uniformly distributed loading.

## Design Parameters

- **Span:** 6 m
- **Dead Load:** 5 kN/m
- **Live Load:** 10 kN/m
- **Steel Grade:** 300 (f_y = 300 MPa)
- **Section:** 310UB46.2 (S_x = 810,000 mm^3)

## Calculations


$$
\begin{alignedat}{2}
L &= 6 \text{ m} \\\\
\text{w_dead} &= 5000 \text{ kg s}^{-2} \\\\
\text{w_live} &= 10000 \text{ kg s}^{-2} \\\\
\text{f_y} &= 300000000 \text{ Pa} \\\\
\text{S_x} &= 8.1E-07 \text{ m}^{3} \\\\
\text{w_uls} &= 1.2 \text{w_dead} + 1.5 \text{w_live} \\
&= 1.2 \times 5,000 \text{ kg s}^{-2} + 1.5 \times 10,000 \text{ kg s}^{-2} \\
&= 21 \times 10^{3} \text{ kg s}^{-2} \\\\
\text{M_star} &= \frac{\text{w_uls} L^{2}}{8} \\
&= \frac{21 \times 10^{3} \text{ kg s}^{-2} \times \left(6 \text{ m}\right)^{2}}{8} \\
&= 94.5 \times 10^{3} \text{ N m} \\\\
\text{phi} &= 0.9 \\\\
\text{M_s} &= \text{f_y} \text{S_x} \\
&= 300 \text{ MPa} \times 810 \text{ mm}^{3} \\
&= 243 \text{ N m} \\\\
\text{phi_M_s} &= \text{phi} \text{M_s} \\
&= 0.9 \times 243 \text{ N m} \\
&= 218.7 \text{ N m} \\\\
\text{utilisation} &= \frac{\text{M_star}}{\text{phi_M_s}} \\
&= \frac{94.5 \times 10^{3} \text{ N m}}{218.7 \text{ N m}} \\
&= 432.1 \\
\end{alignedat}
$$


## Summary

The beam utilisation ratio indicates whether the section is adequate:
- **Utilisation < 1.0:** Section is adequate
- **Utilisation > 1.0:** Section is inadequate, select a larger section
