import React from 'react';

interface ButtonProps {
  onClick: () => void;
  disabled?: boolean;
  loading?: boolean;
  children: React.ReactNode;
}

const Button: React.FC<ButtonProps> = ({ onClick, disabled, loading, children }) => {
  return (
    <button
      onClick={onClick}
      disabled={disabled || loading}
      className={`w-full py-2 px-4 rounded-lg text-white font-semibold transition ${
        loading
          ? "bg-blue-400 animate-pulse cursor-not-allowed"
          : "bg-blue-600 hover:bg-blue-700"
      }`}
    >
      {loading ? "Uploading..." : children}
    </button>
  );
};

export default Button;
