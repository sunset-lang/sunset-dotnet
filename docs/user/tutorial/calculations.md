# Performing calculations

It's been a while since you studied projectile motion (which is odd, given your profession), so you have the castle's
mathematician work out the formulas you'll need to calculate the launch distance.

They provide the following information:

> To calculate the distance that the crate travels, follow these steps:
> 1. Calculate the total mass of the projectile, $m = m_{crate} + m_{cabbages}$
> 2. Calculate the speed at which the crate leaves the catapult, $v_0 = \frac{FT}{m}$
> 3. The distance that the crate travels from the launch point
     is $\left(0.204 \text{m}^{-1}\text{s}^2\right) v_0^2\sin\theta\cos\theta$

On the back of the piece of paper, you find their workings which you can choose to ignore:

> [!NOTE]
> Let's call the mass of the crate $m_{crate}$ and the mass of the cabbages $m_{cabbages}$. If we add this together, we
> get the total mass that needs to be fired:
$$
m = m_{crate} + m_{cabbages}
$$
> Now, we need to work out the initial velocity of the crate as it leaves the catapult. If the catapult can apply a
> force of $F$ to the crate for $T$ time, the acceleration of the crate is:
$$
a = \frac{F}{m}
$$
> After $T$ time from firing, the initial velocity $v_0$ of the crate will be
$$
v_0 = aT = \frac{FT}{m}
$$
> If we fire the crate at an angle of $\theta$ to the horizontal, we can break the initial velocity $v_0$ into vertical
> and horizontal components $v_x(t=0)$ and $v_y(t=0)$, where $t$ is the after the crate is launched.
$$
\begin{align}
v_x(0) &= v_0 \cos{\theta} \\
v_y(0) &= v_0 \sin{\theta}
\end{align}
$$
> We know that the position $(x,y)$, the velocity $(v_x, v_y)$ and the acceleration $(a_x, a_y)$ are related with the
> following differential identities.
$$
\begin{align}
a_x = \frac{dv_x}{dt} = \frac{d^2x}{dt^2} \\
a_y = \frac{dv_y}{dt} = \frac{d^2y}{dt^2} \\
\end{align}
$$
> We know that gravity (a vertical downward acceleration of magnitude $g$) is the only force acting on the crate once it
> leaves the catapult. If we sum up the forces in the vertical ($y$) direction and substitute in our derivatives we get:
$$
\begin{align}
\sum F_y = ma_y &= -mg \\
m\frac{dv_y}{dt} &= -mg \\
\frac{dv_y}{dt} &= -g \\
\end{align}
$$
> Integrating once with respect to $t$ then substituting in $v_y(0)$ to calculate $C_0$, we find
$$
\begin{align}
v_y &= \int -g \space dt \\
&= -gt + C_0 \\
&= -gt + v_0\sin\theta
\end{align}
$$
> Integrating this again and substituting $y(t=0) = 0$, we find the relationship between $y$ and $t$:
$$
\begin{align}
y &= \int -gt + v_0\sin\theta \space dt \\
&= -\frac{1}{2}gt^2 + v_0t\sin\theta + C_1 \\
&= -\frac{1}{2}gt^2 + v_0t\sin\theta
\end{align}
$$
> We need to work out the time $t_{impact}$ when the crate hits the ground again (i.e. when $y=0$ again), so we solve
> the above
> for $y=0$ by factorising:
$$
\begin{align}
&t_{impact}\left(-\frac{1}{2}gt_{impact} + v_0\sin\theta \right) = 0 \\
&t_{impact} = 0,\space \frac{2v_0\sin\theta}{g} \\
\end{align}
$$
> The first solution is obviously when the crate is just taking off, so the second solution is when the crate makes
> contact with the ground again.
> We know that there aren't any forces in the horizontal direction after the crate leaves the catapult, so the sum of
> the horizontal forces is 0. We can now calculate the distance that the crate traves in $t_{impact}$ time.
$$
\begin{align}
\sum F_x = ma_x &= 0 \\
v_x &= \int 0 \space dt \\
&= C_0 \\
&= v_0 \cos\theta \\
x &= \int v_0 \cos\theta \space dt \\
&= v_0 t \cos\theta + C_1 \\
&= v_0 t \cos\theta
\end{align}
$$
> Substituting $t = t_{impact}$ and the acceleration due to gravity $g = 9.81\text{ms}^{-2}$, we come to the final
> solution:
$$
\begin{align}
v_0 &= \frac{FT}{m} \\
x_{impact} &= \frac{2}{g} v_0^2 \sin\theta \cos\theta \\
&= \left(0.204 \text{m}^{-1}\text{s}^2\right) v_0^2\sin\theta\cos\theta
\end{align}
$$

## Performing arithmetic

Let's first work out the total mass of the crate. This is just some simple arithmetic, and this is
what [The Sunset Reference]() says about performing arithmetic:

> ### Numbers
> Numbers can come in the following forms:
>
> - Integers, e.g. `1`, `45`, `-135`
> - Decimal numbers, e.g. `12.94`, `-14.8`
> - Scientific notation, e.g. `4e+6`, -13.45e-9`
>
> ### Arithmetic operations
> These standard arithmetic operators can be used on any numeric quantities:
>
> - `+` for addition
> - `-` for subtraction
> - `*` for multiplication
> - `/` for division
> - `^` for exponentiation (i.e. powers)
>
> Expression can be grouped with parentheses i.e. `(` and `)` to change the order of operations.

If we know that the crate has a mass of 30 kilograms and the cabbages have a mass of 150 kilograms, calculate the total
below by typing in the expression and hitting `CTRL` + `Enter`. You don't need to put an `=` sign in - we'll come back
to that later.

```sunset
30 + 150
```

You should see a result of `180` now, which is the sum of the two numbers. Excellent!

## Using units

However, you note that this number isn't all that meaningful - is it 180 grams, 180 kilograms, 180 tonnes or even 180
meters. You know that it's 180 kilograms because you did the calculation, but if you come back to it 5 years from now
are you sure that you'll still know that?

It looks like Sunset can help you out with how units are handled, let's have a look at what The Sunset Manual has to say
about units.

> ### Units
>
> Units of measurement can be applied to any numbers by placing the short form of the unit into curly braces `{` and `}`
> next to the number. The language contains both "base" units (e.g. `m` for "metres") as well as their multiples (e.g.
`mm` for "millimetres").
>
> The allowed base units are:
> - `g`, `kg`, `T` - Mass units
> - `mm`, `m`, `km` - Length units
> - `ms`, `s`, `min`, `hr` - Time units
>
> Units can be combined with each other using the operators `*` `/` and `^`. `+` and `-` can't be used, as there's no
> real meaning to the addition of two units.
>
> Unlike normal expressions, a space can be used between two units instead of the multiplication symbol `*`.
>
> Examples:
> - 12 kilograms: `12 {kg}`
> - 1.5 metres: `1.5 {m}`
> - 6 metres per second: `6 {m / s}` or `6 {m s^-1}`
> - 3 kilonewtons: `3 {kN}` or `3000 {kg m / s^2}`
>
> Units are automatically worked out by Sunset for calculations, and it will try to give you a unit that results in the
> smallest possible output number magnitude.

To use this newfound knowledge in your calculation, add units to the calculation and check whether the correct units are
applied on the other side.

```sunset
30 {kg} + 150 {kg}
```

This will give you back the answer `180 {kg}`, which is precisely what you're after.

## Finishing the calculations off

Now that we have a calculation under our belt and know how to use units, let's just calculate the answer.

We know that the total mass of the crate and cabbages is 180 kg. Now we plug that into the second formula to get the
initial velocity:

```sunset
3 {kN} * 2 {s} / 180 {kg}
```

This results in # m/s, which is the correct units for velocity. Somehow, behind the scenes Sunset knew that a newton is
really a $\text{kgms}^{-2}$, which when multiplied by a time in seconds and divided by a mass in kilograms results in a
velocity in m / s.

All we need to do is plug these numbers into the final formula, which results in the calculated result of:

```sunset
0.204 {m^-1 s^2} * (# {m /s})^2 * sin(45 {deg}) * cos(45 {deg})
```

And the final result is 35m, which is just short of the total amount required.

Fortunately, you realise that the angle that the catapult fires the crate can be adjusted, and you can put a bigger
counterweight in there to increase the force applied to the crate. This will propel it faster and therefore farther.

You get back into doing the calculations, but realise that it's not quite that easy to work out which number meant what,
and you need to start from the very beginning of the calculations and manually transfer the results from calculation to
calculation as you go.

You have a suspicion that Sunset (and computers in general) can deal with this more gracefully, so let's look at how we
can tidy this up using **variables**.

[Previous](sunset-tutorial.md)
[Next](variables.md)