# Simple Beam Calculation

A quick check of a steel beam.


$$
\begin{alignedat}{2}
L &= 6 \text{ m} \\\\
w &= 21000 \text{ kg s}^{-2} \\\\
\text{f_y} &= 300000000 \text{ Pa} \\\\
\text{S_x} &= 8.1E-07 \text{ m}^{3} \\\\
\text{M_star} &= \frac{w L^{2}}{8} \\
&= \frac{21 \times 10^{3} \text{ kg s}^{-2} \times \left(6 \text{ m}\right)^{2}}{8} \\
&= 94.5 \times 10^{3} \text{ N m} \\\\
\text{M_s} &= \text{f_y} \text{S_x} \\
&= 300 \text{ MPa} \times 810 \text{ mm}^{3} \\
&= 243 \text{ N m} \\\\
\text{utilisation} &= \frac{\text{M_star}}{\text{M_s}} \\
&= \frac{94.5 \times 10^{3} \text{ N m}}{243 \text{ N m}} \\
&= 388.9 \\
\end{alignedat}
$$


The beam utilisation ratio is shown above.
