FROM mcr.microsoft.com/dotnet/core/sdk:3.1

RUN git clone https://github.com/Bingo1392/StratisFullNode.git \
    && cd /StratisFullNode/src/Stratis.SmallD \
	&& dotnet build
	
VOLUME /root/.stratisbitcoin

WORKDIR /StratisFullNode/src/Stratis.SmallD

COPY stratis.conf.docker /root/.stratisnode/small/SmallMain/stratis.conf

EXPOSE 18555 18554 18553 18552

CMD ["dotnet", "run"]