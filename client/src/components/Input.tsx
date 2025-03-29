import React from 'react';

interface InputProps {
  type: string;
  accept?: string;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  value?: string;
}

const Input: React.FC<InputProps> = ({ type, accept, onChange, value }) => {
  return (
    <input
      type={type}
      accept={accept}
      onChange={onChange}
      value={value}
      className="block w-full text-sm text-gray-600 file:mr-4 file:py-2 file:px-4 file:rounded-full file:border-0 file:text-sm file:font-semibold file:bg-blue-100 file:text-blue-700 hover:file:bg-blue-200 transition"
    />
  );
};

export default Input;
