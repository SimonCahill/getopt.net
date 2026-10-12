export ZSH="$HOME/.oh-my-zsh"

ZSH_THEME="jispwoso"
plugins=(git dotnet)

# Update the pinned Oh My Zsh revision in Dockerfile when rebuilding the image.
zstyle ':omz:update' mode disabled

source "$ZSH/oh-my-zsh.sh"
