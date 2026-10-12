FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine

ARG OH_MY_ZSH_COMMIT=083c339e0622bfc144b66ebf1a2e8dbe7d2ada0d

RUN apk add --no-cache \
        bash \
        ca-certificates \
        curl \
        git \
        icu-data-full \
        icu-libs \
        openssh-client \
        procps \
        shadow \
        sudo \
        zsh \
    && addgroup -g 1000 vscode \
    && adduser -D -u 1000 -G vscode -s /bin/zsh vscode \
    && printf 'vscode ALL=(ALL) NOPASSWD:ALL\n' > /etc/sudoers.d/vscode \
    && chmod 0440 /etc/sudoers.d/vscode \
    && mkdir -p /home/vscode/.ssh \
    && chmod 0700 /home/vscode/.ssh \
    && chown vscode:vscode /home/vscode/.ssh

# The parser and its tests use named cultures, which need ICU on Alpine.
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    SHELL=/bin/zsh

USER vscode
WORKDIR /home/vscode

RUN git init /home/vscode/.oh-my-zsh \
    && git -C /home/vscode/.oh-my-zsh remote add origin https://github.com/ohmyzsh/ohmyzsh.git \
    && git -C /home/vscode/.oh-my-zsh fetch --depth 1 origin "${OH_MY_ZSH_COMMIT}" \
    && git -C /home/vscode/.oh-my-zsh checkout --detach FETCH_HEAD \
    && test -f /home/vscode/.oh-my-zsh/themes/jispwoso.zsh-theme

COPY --chown=vscode:vscode .zshrc /home/vscode/.zshrc

CMD ["sleep", "infinity"]
