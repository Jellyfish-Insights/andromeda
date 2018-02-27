# This is the file for the base image of every other image in the
# repo. When changed, it must be built manually and pushed to the
# registry as the following:

# docker pull microsoft/dotnet:2.1.300-sdk
# docker build -t registry.gitlab.com/brick-abode-developers/fee/year/year-ap-dev .
# docker login registry.gitlab.com
# docker push registry.gitlab.com/brick-abode-developers/fee/year/year-ap-dev

# You'll also need to rebuild the other images:

# docker-compose [-f docker-compose.(daemons|mock).yml] build

FROM microsoft/dotnet:2.1.300-sdk

RUN wget -O - https://deb.nodesource.com/setup_9.x | bash - \
    && apt-get install -y \
        build-essential \
        cmake \
        curl \
        git-core \
        nodejs \
        parallel \
        # python dependencies
        libbz2-dev \
        libncurses5-dev \
        libncursesw5-dev \
        libreadline-dev \
        libsqlite3-dev \
        libssl-dev \
        llvm \
        make \
        tk-dev \
        wget \
        xz-utils \
        zlib1g-dev \
    && rm -rf /var/lib/apt/lists/*

RUN npm install -g typescript-formatter typescript tslint tslint-react

RUN git clone https://github.com/uncrustify/uncrustify.git \
    && cd uncrustify && git checkout "uncrustify-0.67" \
    && mkdir build && cd build \
    && cmake .. && make && make install \
    && cd ../../ && rm -rf uncrustify

# install pyenv, python, and pipenv
RUN curl -L https://github.com/pyenv/pyenv-installer/raw/master/bin/pyenv-installer | bash
ENV PATH="/root/.pyenv/bin:$PATH"
RUN eval "$(pyenv init -)" \
    && eval "$(pyenv virtualenv-init -)" \
    && pyenv update \
    && pyenv install 3.6.5 \
    && pyenv global 3.6.5 \
    && pip install --upgrade pip pipenv

# required for pipenv
ENV LC_ALL=C.UTF-8
ENV LANG=C.UTF-8

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

CMD ["/bin/sh"]
