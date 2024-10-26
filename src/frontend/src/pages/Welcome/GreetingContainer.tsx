type GreetingContainerProps = { login: () => void; }

export const GreetingContainer = ({ login }: GreetingContainerProps) => {
  return (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        width: "100%",
        height: "100%",
        border: "5px solid #B3B3B3",
        gap: "4vh",
        background:
          "radial-gradient(50% 50% at 50% 50%, #FFFFFF 61.83%, #D9D9D9 100%)",
      }}
    >
      Greetings fighter! Log in using Telegram!
      <div
        style={{
          width: "60%",
          height: "30%",
          background: "#75757580",
          border: "5px solid #757575",
          borderRadius: "8px",
          padding: "1vh",
        }}
      >
        <DieButton onClick={login} />
      </div>
    </div>
  );
};

type DieButtonProps = {
  onClick: () => void;
};

const DieButton = ({ onClick }: DieButtonProps) => {
  return (
    <img
      onClick={onClick}
      style={{
        maxWidth: "100%",
        maxHeight: "100%",
      }}
      src="./src/assets/die.png"
    />
  );
};
