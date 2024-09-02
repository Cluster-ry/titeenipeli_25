export const GreetingContainer = (props: { login: () => void }) => {
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
      Greetings "Username"
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
        <DieButton onClick={props.login} />
      </div>
      For whom are you fighting for?
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
