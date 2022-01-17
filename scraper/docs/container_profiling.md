# Profiling the container

The `bootstrap.sh` implements a function `system_snapshot`, which runs in the
background and takes a snapshot of CPU and memory usage every 60 seconds.

The following statistics take into consideration `production` version.

## CPU usage

The most CPU-intensive process is not `google-chrome`, as we had expected, but
`java`, the one responsible for `browsermob-proxy`. This sets up a proxy server
which is capable of rewriting requests and saving their output in a JSON format.
The project is since 4 years no longer maintained and is known for several bugs,
especially related to its Python port.

The current workaround is to kill the `java` processes once we are done with
every scraping job.

## Memory usage

Memory usage seems to be stable even if the container has been running for days.
The following values can be expected:

- Baseline (memory usage between two scraping jobs): approximately 20 MB
- Peak memory usage: 900-1000 MB

In terms of memory usage, `google-chrome` and `browsermob-proxy` compete, each
with around 50% of the total usage.
